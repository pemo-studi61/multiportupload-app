# Build-Admin.ps1

Write-Host "=== MultiPortUpload Admin Build ===" -ForegroundColor Cyan

$adminProjectPath = "src/MultiPortUpload.Admin"
$adminTargetPath = "src/MultiPortUpload.Api/wwwroot/admin"

Write-Host "1. React Frontend bauen..." -ForegroundColor Yellow

Push-Location $adminProjectPath

npm run build

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Frontend-Build fehlgeschlagen."
    Pop-Location
    exit 1
}

Pop-Location

Write-Host "2. Vorhandene Dateien löschen..." -ForegroundColor Yellow

if (Test-Path $adminTargetPath)
{
    Remove-Item "$adminTargetPath/*" -Recurse -Force
}

Write-Host "3. Neue Dateien kopieren..." -ForegroundColor Yellow

Copy-Item `
    "$adminProjectPath/dist/*" `
    $adminTargetPath `
    -Recurse `
    -Force

Write-Host "4. Docker Container neu bauen..." -ForegroundColor Yellow

docker compose down
# docker compose up --build -d
docker compose build --no-cache
docker compose up -d

if ($LASTEXITCODE -ne 0)
{
    Write-Error "Docker Build fehlgeschlagen."
    exit 1
}

Write-Host ""
Write-Host "Admin Frontend erfolgreich deployed." -ForegroundColor Green
Write-Host ""
Write-Host "Testen unter:" -ForegroundColor Cyan
Write-Host "http://localhost:8080/admin"