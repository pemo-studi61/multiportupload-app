#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Aktiviert das tc netem Kernel-Modul in der Docker Desktop VM.

.DESCRIPTION
    Dieses Skript lädt das sch_netem Kernel-Modul, das für die Netzwerkemulation
    mit tc netem benötigt wird. Es greift auf die Docker Desktop Linux VM zu
    und lädt das Modul dort.

.EXAMPLE
    ./Enable-TcNetem.ps1

.NOTES
    Erfordert Docker Desktop und funktioniert nur auf macOS/Windows mit Docker Desktop.
    Auf Linux muss das Modul direkt auf dem Host geladen werden: sudo modprobe sch_netem
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Aktiviere tc netem Kernel-Modul in Docker Desktop..." -ForegroundColor Cyan
Write-Host "Dies erfordert, dass das sch_netem Kernel-Modul in der Docker Desktop Linux VM geladen wird." -ForegroundColor Yellow

try {
    # Lade das Kernel-Modul in der Docker Desktop VM
    Write-Host "`nLade sch_netem Kernel-Modul..." -ForegroundColor Cyan
    
    docker run --rm --privileged --pid=host justincormack/nsenter1 `
        /usr/bin/nsenter --mount=/proc/1/ns/mnt -- modprobe sch_netem
    
    if ($LASTEXITCODE -ne 0) {
        throw "Fehler beim Laden des Kernel-Moduls (Exit Code: $LASTEXITCODE)"
    }
    
    Write-Host "✓ sch_netem Kernel-Modul erfolgreich geladen" -ForegroundColor Green
    
    # Verifiziere, dass es geladen wurde
    Write-Host "`nVerifiziere Modul..." -ForegroundColor Cyan
    
    $lsmodOutput = docker run --rm --privileged --pid=host justincormack/nsenter1 `
        /usr/bin/nsenter --mount=/proc/1/ns/mnt -- lsmod 2>&1
    
    if ($lsmodOutput -match "sch_netem") {
        Write-Host "✓ Modul in lsmod gefunden" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Modul nicht in lsmod sichtbar, könnte aber trotzdem funktionieren" -ForegroundColor Yellow
    }
    
    Write-Host "`n✓ tc netem sollte jetzt in Containern mit NET_ADMIN Capability verfügbar sein" -ForegroundColor Green
}
catch {
    Write-Host "`n✗ Fehler beim Aktivieren von tc netem:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "`nHinweis: Auf nativem Linux verwenden Sie: sudo modprobe sch_netem" -ForegroundColor Yellow
    exit 1
}
