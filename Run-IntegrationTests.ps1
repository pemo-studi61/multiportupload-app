<#
.SYNOPSIS
    Startet die Python-Integrationstests mit Docker Compose in einem Schritt.

.DESCRIPTION
    Das Skript startet API und MinIO, führt die PyTests im upload-testrunner
    aus und schreibt JUnit- sowie Cobertura-Reports nach test-results/ und
    coverage/. Optional können Services nach dem Lauf aktiv bleiben.

.PARAMETER KeepServicesUp
    Lässt API/MinIO nach dem Testlauf aktiv.

.PARAMETER NoBuild
    Überspringt den Build beim docker compose up.

.EXAMPLE
    ./Run-IntegrationTests.ps1

.EXAMPLE
    ./Run-IntegrationTests.ps1 -KeepServicesUp
#>

[CmdletBinding()]
param(
    [switch] $KeepServicesUp,
    [switch] $NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

$composeArgs = @(
    "-f", "docker-compose.yml",
    "-f", "docker-compose.testrunner.yml"
)

$upArgs = @("up")
if (-not $NoBuild) {
    $upArgs += "--build"
}
$upArgs += @("-d", "multiportupload-api", "minio")

New-Item -ItemType Directory -Force -Path "test-results", "coverage" | Out-Null

Write-Host "[1/3] Starte API + MinIO..." -ForegroundColor Cyan
& docker compose @composeArgs @upArgs

try {
    Write-Host "[2/3] Fuehre PyTests aus..." -ForegroundColor Cyan
    & docker compose @composeArgs run --rm --workdir /workspace upload-testrunner `
        -NoProfile -Command "New-Item -ItemType Directory -Force -Path /workspace/test-results,/workspace/coverage | Out-Null; python3 -m pytest tests/integration --junitxml=/workspace/test-results/pytest.junit.xml --cov=tests/integration --cov-report=term --cov-report=xml:/workspace/coverage/Cobertura.xml"

    $testExitCode = $LASTEXITCODE
}
finally {
    if (-not $KeepServicesUp) {
        Write-Host "[3/3] Stoppe Compose-Services..." -ForegroundColor Cyan
        & docker compose @composeArgs down
    }
    else {
        Write-Host "[3/3] Services bleiben aktiv (-KeepServicesUp)." -ForegroundColor Yellow
    }
}

if (Test-Path "test-results/pytest.junit.xml") {
    Write-Host "JUnit Report: test-results/pytest.junit.xml" -ForegroundColor Green
}
if (Test-Path "coverage/Cobertura.xml") {
    Write-Host "Coverage Report: coverage/Cobertura.xml" -ForegroundColor Green
}

exit $testExitCode
