<#
.SYNOPSIS
    Proves a backup works: restores it into a throwaway database on this computer and opens every
    saved request with the certificate.

.DESCRIPTION
    A backup nobody has restored is only a hope. This starts a temporary Postgres in Docker, restores
    the backup into it, prints how many rows came back, then runs the site's own check
    (dotnet run -- --check-decryption) against it with the data protection certificate. That proves the
    backup and the certificate together still open what people wrote. It prints counts only, never
    anybody's text. The temporary database is removed afterwards.

    Needs Docker Desktop and the .NET SDK. Run from the repository root.

.PARAMETER Backup
    The .dump file to restore. Default: the newest one in pab-backups in your home folder.

.PARAMETER CertificateDir
    Where New-KeyCertificate.ps1 put the certificate files. Default: pab-secrets in your home folder.

.PARAMETER PostgresMajor
    The Postgres version to restore into. It must be at least the version the backup was taken from.
    Default 18, which is what Neon runs at the moment.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\tools\database\Test-Restore.ps1
#>
param(
    [string]$Backup,
    [string]$CertificateDir = (Join-Path $HOME 'pab-secrets'),
    [int]$PostgresMajor = 18
)

$ErrorActionPreference = 'Stop'

if (-not $Backup) {
    $latest = Get-ChildItem (Join-Path $HOME 'pab-backups') -Filter 'pab-backup-*.dump' -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $latest) { throw 'No backup found. Run Backup-Database.ps1 first, or pass -Backup.' }
    $Backup = $latest.FullName
}
$Backup = (Resolve-Path $Backup).Path

$certificateFile = Join-Path $CertificateDir 'DataProtection__Certificate.txt'
$passwordFile = Join-Path $CertificateDir 'DataProtection__CertificatePassword.txt'
if (-not (Test-Path $certificateFile) -or -not (Test-Path $passwordFile)) {
    throw "The certificate files are not in $CertificateDir. Restore them from the password manager (the two .txt files that New-KeyCertificate.ps1 made), or pass -CertificateDir."
}

# Windows PowerShell treats anything a program writes to stderr as an error once errors are fatal, so
# the calls whose complaints don't matter (removing a container that isn't there) go through this.
function Quietly([scriptblock]$Command) {
    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try { & $Command *> $null } finally { $ErrorActionPreference = $previous }
}

$container = 'pab-restore-test'
$port = 5434
$password = [Guid]::NewGuid().ToString('N')
$failed = $false

Quietly { docker rm -f $container }
try {
    Write-Host "Starting a temporary Postgres $PostgresMajor ..."
    docker run -d --name $container -e "POSTGRES_PASSWORD=$password" -p "127.0.0.1:${port}:5432" `
        -v "${Backup}:/restore/backup.dump:ro" "postgres:$PostgresMajor" | Out-Null
    # The image starts a temporary server to set itself up, stops it, and starts the real one, so
    # "ready to accept connections" shows up twice. Only after the second is it safe to connect.
    for ($i = 0; $i -lt 90; $i++) {
        $ready = (cmd /c "docker logs $container 2>&1" | Select-String 'ready to accept connections').Count
        if ($ready -ge 2) { break }
        Start-Sleep -Seconds 1
    }
    docker exec $container psql -U postgres -c 'CREATE DATABASE pab' | Out-Null

    Write-Host "Restoring $Backup ..."
    docker exec $container pg_restore --no-owner --no-privileges --exit-on-error -U postgres -d pab /restore/backup.dump
    if ($LASTEXITCODE -ne 0) { throw "pg_restore failed (exit $LASTEXITCODE)." }

    Write-Host ''
    Write-Host 'Rows that came back:'
    # Windows PowerShell drops double quotes when it hands text to a program, and Postgres folds
    # unquoted names to lower case, so each quote is escaped.
    $counts = 'SELECT (SELECT count(*) FROM "SupportRequests") AS support_requests, (SELECT count(*) FROM "ContactInquiries") AS contact_messages, (SELECT count(*) FROM "DataProtectionKeys") AS keys, (SELECT count(*) FROM "__EFMigrationsHistory") AS migrations'
    docker exec $container psql -U postgres -d pab -c $counts.Replace('"', '\"')

    Write-Host 'Opening every saved request with the certificate ...'
    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    # Only the summary is wanted. A wrong certificate would otherwise print the same stack trace ten times.
    $env:Logging__LogLevel__Default = 'Warning'
    ${env:Logging__LogLevel__Microsoft.AspNetCore.DataProtection} = 'None'
    ${env:Logging__LogLevel__PhilisaAbantuBethu.Data.Protection} = 'None'
    $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=$port;Database=pab;Username=postgres;Password=$password"
    $env:DataProtection__Certificate = (Get-Content $certificateFile -Raw).Trim()
    $env:DataProtection__CertificatePassword = (Get-Content $passwordFile -Raw).Trim()
    dotnet run --project (Join-Path $PSScriptRoot '..\..\PhilisaAbantuBethu') --no-launch-profile -- --check-decryption
    if ($LASTEXITCODE -ne 0) { $failed = $true }
}
finally {
    Remove-Item Env:ConnectionStrings__DefaultConnection, Env:DataProtection__Certificate, `
        Env:DataProtection__CertificatePassword, Env:ASPNETCORE_ENVIRONMENT, Env:Logging__LogLevel__Default, 'Env:Logging__LogLevel__Microsoft.AspNetCore.DataProtection', 'Env:Logging__LogLevel__PhilisaAbantuBethu.Data.Protection' -ErrorAction SilentlyContinue
    Quietly { docker rm -f $container }
}

Write-Host ''
if ($failed) {
    Write-Host 'RESTORE TEST FAILED: the backup restored but its text could not be opened. Is this the certificate that was live when the backup was taken?' -ForegroundColor Red
    exit 1
}
Write-Host 'RESTORE TEST PASSED: the backup restores and every saved request opens.' -ForegroundColor Green
