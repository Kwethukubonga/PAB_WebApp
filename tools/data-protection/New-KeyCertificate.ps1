<#
.SYNOPSIS
    Makes the certificate that locks the data protection keys stored in the database.

.DESCRIPTION
    On Postgres the keys that encrypt the situation text are kept in the database. They are encrypted
    with a certificate, so a copy of the database on its own can't be read. The certificate's private
    half lives only in the host's secret settings.

    This writes three files to a folder OUTSIDE the repository (default: pab-secrets in your home
    folder):

      pab-data-protection.pfx                              the certificate, for your backup
      DataProtection__Certificate.txt                      the same thing as text, for the host
      DataProtection__CertificatePassword.txt              the password for it

    The two .txt files are named after the host settings they go into. Open each, copy the whole
    text, and paste it as the value of the setting with that name (Render: Environment).

    KEEP A BACKUP OF THE .pfx AND THE PASSWORD IN THE TEAM'S PASSWORD MANAGER. If the certificate is
    lost, the keys in the database can never be opened again and every saved support request becomes
    unreadable. Never put these files in Git, in a chat or in an email.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\tools\data-protection\New-KeyCertificate.ps1
#>
param(
    [string]$OutDir = (Join-Path $HOME 'pab-secrets')
)

$ErrorActionPreference = 'Stop'

$pfxPath = Join-Path $OutDir 'pab-data-protection.pfx'
if (Test-Path $pfxPath) {
    throw "$pfxPath already exists. A second certificate would not open keys locked with the first, so this will not overwrite it."
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$rsa = [System.Security.Cryptography.RSA]::Create(3072)
$request = New-Object System.Security.Cryptography.X509Certificates.CertificateRequest(
    'CN=PAB data protection',
    $rsa,
    [System.Security.Cryptography.HashAlgorithmName]::SHA256,
    [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
$usage = [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment -bor `
         [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DataEncipherment
$request.CertificateExtensions.Add(
    (New-Object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension($usage, $true)))
$certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddDays(-1), [DateTimeOffset]::UtcNow.AddYears(10))

# A random password made only of letters and digits, so it survives being pasted anywhere.
$random = New-Object byte[] 24
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($random)
$password = ([Convert]::ToBase64String($random)) -replace '[^A-Za-z0-9]', 'x'

$pfx = $certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $password)
[System.IO.File]::WriteAllBytes($pfxPath, $pfx)
[System.IO.File]::WriteAllText((Join-Path $OutDir 'DataProtection__Certificate.txt'), [Convert]::ToBase64String($pfx))
[System.IO.File]::WriteAllText((Join-Path $OutDir 'DataProtection__CertificatePassword.txt'), $password)

Write-Host ''
Write-Host "Done. The files are in: $OutDir"
Write-Host ''
Write-Host '1. Copy the files into the team password manager now (the .pfx and the password).'
Write-Host '2. In Render: Environment. Add DataProtection__Certificate with the text from'
Write-Host '   DataProtection__Certificate.txt, and DataProtection__CertificatePassword with the text'
Write-Host '   from DataProtection__CertificatePassword.txt.'
Write-Host '3. Keep the folder out of Git. Delete the .txt files once the backup and Render are done.'
Write-Host ''
Write-Host 'Do not paste any of it into a chat.'
