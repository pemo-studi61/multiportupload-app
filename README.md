# MultiPortUpload

Ein .NET-9-Microservice, der Datei-Uploads über **mehrere austauschbare
Upload-Strategien** ("Ports"/Adapter) abwickelt und die Performance jeder
Variante misst (Benchmarking). Derselbe Upload kann zur Laufzeit über
verschiedene Backends laufen — lokale Platte, S3/MinIO, In-Memory, presigned
URLs, chunked/resumable — und so vergleichbar gemacht werden.

Das Projekt folgt der **Clean Architecture** (Ports & Adapter); die
Schichtgrenzen werden statisch mit NetArchTest erzwungen. Eine ausführliche
Beschreibung findest du in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

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
