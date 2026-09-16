namespace CotelNet.Application.Administration;

public sealed record LegacyCatalogImportResult(
    int PostalServices,
    int Destinations,
    int WeightBands,
    int SupplementaryServices,
    int SupplementaryOptions,
    IReadOnlyList<string> Warnings);

public interface ILegacyCatalogImportService
{
    Task<bool> IsConfiguredAsync(CancellationToken cancellationToken);
    Task<LegacyCatalogImportResult> ImportAsync(CancellationToken cancellationToken);
}
