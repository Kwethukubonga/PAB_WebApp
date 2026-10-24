<#
.SYNOPSIS
    Takes a backup of the live Postgres database to a file on this computer.

.DESCRIPTION
    Uses pg_dump from a Postgres Docker image, so nothing needs installing but Docker Desktop.

    The connection string is a secret. The script asks for it and does not show it; it never goes on
    a command line, into a file or into Git. Use the DIRECT connection string from the Neon console
    (Connect, then turn "Connection pooling" off): pg_dump does not work through the pooler.

    The backup holds names, phone numbers and email addresses in plain form (only the "situation" text
    is encrypted, and its keys are in the backup too, locked by the certificate). Keep the file
    somewhere private: never in the repository, a chat, an email or a shared drive.

.PARAMETER OutDir
    Where backups are written. Default: pab-backups in your home folder.

.PARAMETER PostgresMajor
    The major version of Postgres to run pg_dump from. It must be at least the database's own version
    (Neon's console shows it). Default 18, which is what Neon runs at the moment.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\tools\database\Backup-Database.ps1
#>
param(
    [string]$OutDir = (Join-Path $HOME 'pab-backups'),
    [int]$PostgresMajor = 18
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker is not installed or not on the path. Start Docker Desktop first.'
}

# Asked for, unless PAB_DATABASE_URL is already set in this session (for scheduled or scripted use).
$asked = [string]::IsNullOrWhiteSpace($env:PAB_DATABASE_URL)
if ($asked) {
    $secure = Read-Host -Prompt 'Paste the database connection string (it will not be shown)' -AsSecureString
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        $env:PAB_DATABASE_URL = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}
if ([string]::IsNullOrWhiteSpace($env:PAB_DATABASE_URL)) { throw 'No connection string given.' }

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$name = 'pab-backup-{0}.dump' -f (Get-Date -Format 'yyyyMMdd-HHmmss')

try {
    # Custom format: compressed, and pg_restore can pick out what it needs. No owners or grants,
    # so it restores into any database whatever its user is called.
    docker run --rm -e PAB_DATABASE_URL -v "${OutDir}:/backup" "postgres:$PostgresMajor" `
        sh -c "pg_dump --format=custom --no-owner --no-privileges --dbname=`"`$PAB_DATABASE_URL`" --file=/backup/$name"
    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump failed (exit $LASTEXITCODE). If it says the server version is newer, run again with -PostgresMajor set to the database's version."
    }
}
finally {
    if ($asked) { Remove-Item Env:PAB_DATABASE_URL -ErrorAction SilentlyContinue }
}

$path = Join-Path $OutDir $name
$entries = (docker run --rm -v "${OutDir}:/backup" "postgres:$PostgresMajor" pg_restore --list "/backup/$name" | Measure-Object -Line).Lines
Write-Host ''
Write-Host "Backup written: $path ($([math]::Round((Get-Item $path).Length / 1KB, 1)) KB, $entries items in it)"
Write-Host 'Next: prove it restores, with  powershell -ExecutionPolicy Bypass -File .\tools\database\Test-Restore.ps1'
