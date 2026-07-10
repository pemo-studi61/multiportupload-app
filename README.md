# MultiPortUpload

Letzte Aktualisierung: **10/07/2026**

## Auswertung der Messdaten über ein Jupyter-Notebook

Die Auswertung der bereits vorliegenden Benchmarkdaten (aktuell stehen nur drei Benchmarkläufe mit jeweils 800 BenchmarkRecords zur Auswahl) in Gestalt einer JSON-Datei, die alle BenchmarkRecords enthält, können im Rahmen eines Juypter-Notebooks durchgeführt werden:

[Notebook für die Auswertung der Benchmarkdaten](https://colab.research.google.com/drive/1l1XEgywhpBlBBGaDxgxJXOZ2nD5RG_j8?usp=sharing)

## Projektbeschreibung

*MultiPortUpload* ist ein auf ASP.NET 9 basierender Webservice, der eine REST-API anbietet. Der Webservice wird auf *DigitalOcean* gehostet und kann über die Public-IP direkt angesprochen werden.

Aufruf der "Health-Checks":

```Bash
curl http://157.230.100.116:8080/health
```

Aufruf der API-Dokumentation im Browser:

```Bash
http://157.230.100.116:8080/api/docs
```

Der Hauptzweck von *MultiPortUpload* ist seine Rolle als Backend für das Durchführen von Benchmarks. Dabei werden Dateien hochgeladen und es wird die Ausführungszeit gemessen. Für das Durchführen von Bechmarks gibt es mit `multiportupload-eval` ein eigenes Projekt. Dieses besteht aus einem Satz von PowerShell-Skripten. Da sie auf *PowerShell 7* basieren, können sie auch unter *Linux* oder *MacOS* ausgeführt werden (alle Benchmarktests für die Masterarbeit wurden unter *MacOS* durchgeführt).

Wurde das Projekt geclont, muss eventuell das Modul *PowerShell-Yaml* nachinstalliert werden:

```PowerShell
Install-Module PowerShell-Yaml -Force
```
Der BenchmarkRunner wird über das Skript `Start-BenchmarkLauncher.ps1` gestartet:

```Bash
pwsh ./Start-BenchmarkLauncher.ps1
```

Voraussetzung ist natürlich, dass (auch unter Windows 11) zuvor *PowerShell 7* installiert wurde.

Sollte sich die IP-Adresse der Multiport-Anwendung geändert haben, z.B. weil die Anwendung lokal ausgeführt wird, wird dies in der Konfigurationsdatei `benchmark-config.psd1` eingetragen.

Nach dem Start von `Start-BenchmarkLauncher.ps1?` wird ein Auswahlmenü angeboten. Ein vollständiger Benchmarklauf mit 800 Uploads wird über den Menüpunkt **FullRun** gestartet.

**Wichtig:** Ein FullRun dauert über 13 Stunden!

**Tipp:** Sollte der Server per SSH angesprochen werden, ist es absolut notwendig, dafür *tmux* zu verwenden, da ansonsten bei einem Sessionabbruch alle Daten verloren wären.

---

## Tech-Stack

- **.NET 9.0** / **ASP.NET Core 9** (Kestrel, Minimal APIs + Controller)
- **EF Core 9** + Npgsql (PostgreSQL) für die Benchmark-Persistenz (optional)
- **AWSSDK.S3** für S3 / MinIO
- **NLog** (Logging), **OpenTelemetry** (Metriken, Prometheus-Export)
- **Swagger / Swashbuckle** für die API-Doku
- **React + Vite** Admin-Frontend (`src/MultiPortUpload.Admin`)
- **Docker** / Docker Compose (API + MinIO + optional k6)

---

## Voraussetzungen

- [Docker](https://www.docker.com/) inkl. Docker Compose — empfohlener Weg
- [.NET 9 SDK](https://dotnet.microsoft.com/) — für lokale Entwicklung ohne Docker
- [Node.js](https://nodejs.org/) — nur zum Bauen des Admin-Frontends

---

## Quickstart (Docker)

Der schnellste Weg. Startet die API auf Port **8080** zusammen mit MinIO
(S3-kompatibler Speicher).

```bash
docker compose up --build -d
```

Danach erreichbar:

| Dienst                | URL                              |
| --------------------- | -------------------------------- |
| API                   | http://localhost:8080            |
| Swagger / OpenAPI     | http://localhost:8080/swagger    |
| Admin-Frontend        | http://localhost:8080/admin      |
| Prometheus-Metriken   | http://localhost:8080/metrics    |
| MinIO-Konsole         | http://localhost:9001 (`minioadmin` / `minioadmin`) |

Logs folgen:

```bash
docker compose logs -f multiportupload-api
```

Stoppen:

```bash
docker compose down
```

> **Hinweis:** Die `docker-compose.yml` aktiviert die Benchmark-Persistenz mit
> einer voreingestellten PostgreSQL-Verbindung. Soll keine Datenbank verwendet
> werden, setze `BenchmarkStorage__Enabled` auf `"false"` — die API startet dann
> mit einem No-Op-Store (siehe [docs/SUPABASE.md](docs/SUPABASE.md)).

---

## Quickstart (lokal, ohne Docker)

```bash
# Abhängigkeiten wiederherstellen und bauen
dotnet build

# API starten
dotnet run --project src/MultiPortUpload.Api
```

Konfiguration über `src/MultiPortUpload.Api/appsettings.json` bzw.
`appsettings.Development.json` (Sektionen `LocalStorage`, `S3Storage`,
`BenchmarkStorage`). Ohne erreichbare PostgreSQL-DB läuft die API mit dem
No-Op-Benchmark-Store.

---

## Erster Upload

Eine Datei über eine bestimmte Variante hochladen (z. B. `LocalFile`):

```bash
curl -F "file=@./meinedatei.bin" http://localhost:8080/api/uploads/LocalFile
```

Verfügbare Upload-Varianten auflisten:

```bash
curl http://localhost:8080/api/upload-adapters
```

Mögliche Varianten u. a.: `LocalFile`, `Streaming`, `Memory`, `Hashing`,
`VirusScanMock`, `S3`, `QueueBased`, `Resumable`, `Chunked`, `S3Presigned`.
Details zu den einzelnen Adaptern in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

Benchmark-Ergebnisse abfragen (sofern Persistenz aktiv):

```bash
curl http://localhost:8080/api/benchmarks/summary
```

---

## Tests

```bash
pwsh ./Run-IntegrationTests.ps1
```

Das Skript startet API + MinIO per Docker Compose, führt die PyTest-
Integrationstests aus und erzeugt Reports unter `test-results/` und
`coverage/`.

Optional:

```bash
pwsh ./Run-IntegrationTests.ps1 -KeepServicesUp
```

---

## Admin-Frontend bauen

Das React-Frontend liegt unter `src/MultiPortUpload.Admin` und wird beim Build
nach `src/MultiPortUpload.Api/wwwroot/admin` kopiert.

```bash
cd src/MultiPortUpload.Admin
npm install
npm run build
```

Unter Windows automatisiert das `Run-AdminPanelBuild.ps1` den Build inkl.
Kopieren und Docker-Neubau.

---

## Projektstruktur

```
src/
├── MultiPortUpload.Domain/          Kern-Entitäten (keine Frameworks)
├── MultiPortUpload.Application/      Use Cases & Port-Abstraktionen
├── MultiPortUpload.Infrastructure/   Adapter, Persistenz, S3, Queue, DI
├── MultiPortUpload.Api/              HTTP-Schicht (Controller/Endpoints/Hosting)
└── MultiPortUpload.Admin/            React-/Vite-Admin-Frontend
tests/
└── MultiPortUpload.Tests/            xUnit-Tests inkl. Architekturregeln
docs/
├── ARCHITECTURE.md                   Architektur im Detail
└── SUPABASE.md                       PostgreSQL-/Supabase-Konfiguration
```

---

## Weiterführende Dokumentation

- [Architektur](docs/ARCHITECTURE.md) — Schichten, Ports/Adapter, Abläufe
- [Supabase/PostgreSQL](docs/SUPABASE.md) — Konfiguration der Benchmark-Persistenz
