// file: src/MultiPortUpload.Infrastructure/Adapters/UploadAdapterRegistry.cs

using MultiPortUpload.Application.Ports.Outbound;
using Microsoft.Extensions.Logging;

namespace MultiPortUpload.Infrastructure.Services;

public sealed class UploadAdapterRegistry : IUploadAdapterRegistry
{
    private readonly IReadOnlyDictionary<string, IUploadAdapter> _adapters;

    public UploadAdapterRegistry(IEnumerable<IUploadAdapter> adapters,
    ILogger<UploadAdapterRegistry> logger)
    {

        // PM: 26/05/2026 - Nur zu Debug-Zwecken, damit wir sehen können, welche UploadAdapter tatsächlich registriert wurden. Es könnte sein, dass wir vergessen haben, einen Adapter zu registrieren, oder dass es ein Problem mit der Registrierung gibt, das dazu führt, dass nicht alle Adapter verfügbar sind. Durch die Ausgabe der registrierten Adapter können wir schnell überprüfen, ob alle erwarteten Adapter vorhanden sind und ob ihre Namen korrekt sind.   
        var adapterList = adapters.ToList();

        logger.LogInformation(
            "Registered upload adapters: {Adapters}",
            string.Join(", ", adapterList.Select(a => a.Variant)));

        // Adapter können durch Mehrfachregistrierung in der DI (Program.cs und
        // AddInfrastructure) doppelt vorkommen. Pro Variante den ersten Adapter
        // behalten, damit das Dictionary nicht an doppelten Schlüsseln scheitert.
        _adapters = adapterList
            .GroupBy(adapter => adapter.Variant, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);
    }

    public IUploadAdapter GetByVariant(string variant)
    {
        if (_adapters.TryGetValue(variant, out var adapter))
        {
            return adapter;
        }

        throw new InvalidOperationException(
            $"Keine Upload-Variante mit dem Namen '{variant}' gefunden.");
    }

    public IReadOnlyCollection<IUploadAdapter> GetAll()
    {
        return _adapters.Values.ToList();
    }
}