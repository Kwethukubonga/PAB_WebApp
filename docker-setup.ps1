# Build the Help Centre, prepare local HTTPS, and open it in the browser.
#
# Double-click docker-setup.bat to run this. It generates a trusted development certificate so the
# site can be served over HTTPS at https://localhost:8443/, exports it where docker-compose.yml
# expects it, remembers an encrypted password outside the repository, then builds and starts the
# container and waits until the site answers.
$ErrorActionPreference = "Stop"
Set-Location -LiteralPath $PSScriptRoot

$previousPassword = $env:PHILISA_CERT_PASSWORD

try {
    foreach ($command in @("docker", "dotnet")) {
        if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
            throw "$command was not found. Install Docker Desktop and the .NET 10 SDK, then reopen the launcher."
        }
    }

    Write-Host "Checking Docker..." -ForegroundColor Cyan

    docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Start Docker Desktop, wait until it is ready, then try again."
    }

    docker compose version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose is unavailable. Check your Docker Desktop installation."
    }

    $sdkVersions = dotnet --list-sdks
    if ($LASTEXITCODE -ne 0 -or -not ($sdkVersions -match "^10\.")) {
        throw "Install the .NET 10 SDK before running this launcher."
    }

    $certificateFolder = Join-Path $env:USERPROFILE ".aspnet\https\philisa"
    $settingsFolder = Join-Path $env:LOCALAPPDATA "PhilisaAbafaziBethu"
    $passwordPath = Join-Path $settingsFolder "certificate-password.txt"
    $certificatePath = Join-Path $certificateFolder "philisa.pfx"

    New-Item -ItemType Directory -Force -Path $certificateFolder | Out-Null
    New-Item -ItemType Directory -Force -Path $settingsFolder | Out-Null

    if (Test-Path -LiteralPath $passwordPath) {
        # Windows decrypts this using the current user's account, so it only works as this user.
        $encryptedPassword = (Get-Content -LiteralPath $passwordPath -Raw).Trim()
        $securePassword = ConvertTo-SecureString -String $encryptedPassword
    }
    else {
        $securePassword = Read-Host "Choose a local certificate password" -AsSecureString
        if ($securePassword.Length -eq 0) {
            throw "The certificate password cannot be empty."
        }
    }

    $env:PHILISA_CERT_PASSWORD =
        [System.Net.NetworkCredential]::new("", $securePassword).Password

    Write-Host "Preparing the local HTTPS certificate..." -ForegroundColor Cyan
    Write-Host "Windows may ask you to trust the development certificate."

    dotnet dev-certs https --trust
    if ($LASTEXITCODE -ne 0) {
        throw "The development certificate could not be trusted."
    }

    dotnet dev-certs https --check --trust
    if ($LASTEXITCODE -ne 0) {
        throw "A trusted development certificate is required."
    }

    dotnet dev-certs https --export-path $certificatePath --password $env:PHILISA_CERT_PASSWORD
    if ($LASTEXITCODE -ne 0) {
        throw "The certificate could not be exported."
    }

    if (-not (Test-Path -LiteralPath $certificatePath)) {
        throw "The exported certificate file was not found."
    }

    # Save the password encrypted, outside the repository, so the launcher doesn't ask again.
    $securePassword |
        ConvertFrom-SecureString |
        Set-Content -LiteralPath $passwordPath -Encoding ASCII

    Write-Host "Building and starting the Help Centre..." -ForegroundColor Cyan

    docker compose up --build -d
    if ($LASTEXITCODE -ne 0) {
        throw "Docker could not start the app. Check the build output above."
    }

    # /health answers 200 only once the database is reachable, so it is a better readiness signal
    # than the home page, which would answer before the app has finished starting.
    $healthUrl = "https://localhost:8443/health"
    $clientUrl = "https://localhost:8443/"
    $ready = $false
    $deadline = (Get-Date).AddSeconds(90)

    Write-Host "Waiting for the site..." -ForegroundColor Cyan

    while ((Get-Date) -lt $deadline) {
        try {
            $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 3
            if ($response.StatusCode -eq 200) {
                $ready = $true
                break
            }
        }
        catch {
            # The app may still be starting.
        }
        Start-Sleep -Seconds 2
    }

    if (-not $ready) {
        throw "The site did not become ready. Run 'docker compose logs --tail 40 web' to inspect the app."
    }

    Write-Host "Opening the Help Centre..." -ForegroundColor Green
    Start-Process $clientUrl
}
catch {
    Write-Host "Setup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    $env:PHILISA_CERT_PASSWORD = $previousPassword
}
