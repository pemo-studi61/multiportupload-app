<#
 .SYNOPSIS
 Startet den Docker Container für die Netzwerkbenchmark-Suite, der die Upload-Benchmarks unter verschiedenen Netzwerkbedingungen ausführt und die Ergebnisse speichert.
#>

# rm = "remove" - entfernt den Container nach Beendigung
function Start-BenchmarkSuite {
    Write-Information "[INFO] Starte Netzwerkbenchmark-Suite..." -InformationAction Continue
    docker compose -f docker-compose.yml `
     -f docker-compose.testrunner.yml `
     run --rm upload-testrunner
    if ($LASTEXITCODE -ne 0) {
        Write-Information "[ERROR] Fehler beim Ausführen der Netzwerkbenchmark-Suite." -InformationAction Continue
        exit $LASTEXITCODE
    }
    Write-Information "[INFO] Netzwerkbenchmark-Suite abgeschlossen." -InformationAction Continue
}

Start-BenchmarkSuite