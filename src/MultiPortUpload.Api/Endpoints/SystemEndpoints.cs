// file: src/MultiPortUpload.Api/Endpoints/SystemEndpoints.cs

using System.Reflection;
using System.Runtime.InteropServices;

namespace MultiPortUpload.Api.Endpoints;

/// <summary>
/// Systeminformationen (API-Version, .NET-Runtime) und Zugriff auf die Logdateien.
/// </summary>
public static class SystemEndpoints
{
    public static void MapSystemEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/api/system/info",
            () =>
            {
                var assembly = Assembly.GetEntryAssembly() ?? typeof(SystemEndpoints).Assembly;

                // Informational Version (z. B. "0.1.0+<commit>") bevorzugen, sonst
                // auf die Assembly-Version zurückfallen.
                var apiVersion =
                    assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? assembly.GetName().Version?.ToString()
                    ?? "unknown";

                return Results.Ok(new
                {
                    apiVersion,
                    dotnetVersion = Environment.Version.ToString(),
                    framework = RuntimeInformation.FrameworkDescription,
                    os = RuntimeInformation.OSDescription,
                    timestampUtc = DateTime.UtcNow
                });
            })
            .WithName("SystemInfo")
            .WithTags("System");

        // Listet die verfügbaren Logdateien (Name, Größe, Änderungsdatum).
        app.MapGet(
            "/api/system/logs",
            () =>
            {
                var directory = ResolveLogsDirectory();
                if (!Directory.Exists(directory))
                {
                    return Results.Ok(new { directory, files = Array.Empty<object>() });
                }

                var files = new DirectoryInfo(directory)
                    .GetFiles("*.log")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .Select(f => new
                    {
                        name = f.Name,
                        sizeBytes = f.Length,
                        lastModifiedUtc = f.LastWriteTimeUtc
                    });

                return Results.Ok(new { directory, files });
            })
            .WithName("SystemLogs")
            .WithTags("System");

        // Gibt die letzten N Zeilen einer Logdatei als Text zurück (Standard: 200).
        app.MapGet(
            "/api/system/logs/{file}",
            (string file, int? tail) =>
            {
                // Pfad-Traversal verhindern: nur reine Dateinamen ohne
                // Verzeichnisanteile zulassen.
                var safeName = Path.GetFileName(file);
                if (string.IsNullOrEmpty(safeName) || safeName != file)
                {
                    return Results.BadRequest("Ungültiger Dateiname.");
                }

                var directory = ResolveLogsDirectory();
                var fullPath = Path.Combine(directory, safeName);
                if (!File.Exists(fullPath))
                {
                    return Results.NotFound($"Log-Datei nicht gefunden: {safeName}");
                }

                var lineCount = Math.Clamp(tail ?? 200, 1, 10000);

                // Datei zeilenweise lesen und nur die letzten N Zeilen behalten
                // (speicherbegrenzt). FileShare.ReadWrite, da NLog die Datei
                // gleichzeitig zum Schreiben offen hält.
                var lines = new Queue<string>(lineCount);
                using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) is not null)
                    {
                        if (lines.Count == lineCount)
                        {
                            lines.Dequeue();
                        }
                        lines.Enqueue(line);
                    }
                }

                return Results.Text(string.Join("\n", lines) + "\n", "text/plain");
            })
            .WithName("SystemLogTail")
            .WithTags("System");
    }

    // Ermittelt das Log-Verzeichnis identisch zur NLog-Konfiguration:
    // ${MPU_LOG_ROOT}/logs, ersatzweise ${currentdir}/../../logs.
    private static string ResolveLogsDirectory()
    {
        var logRoot = Environment.GetEnvironmentVariable("MPU_LOG_ROOT");
        if (string.IsNullOrWhiteSpace(logRoot))
        {
            logRoot = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", ".."));
        }
        return Path.Combine(logRoot, "logs");
    }
}
