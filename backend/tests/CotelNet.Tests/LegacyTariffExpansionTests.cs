using CotelNet.Infrastructure.Administration;

namespace CotelNet.Tests;

public sealed class LegacyTariffExpansionTests
{
    [Fact]
    public void ExpandTariff_ReproducesIncrementalLegacyFormula()
    {
        var tariff = new LegacyCatalogImportService.LegacyTariff(
            1, 1, 15, 170, 1, 0.001m, 2m, 0.002m, 3, 0.35m, 0.20m, 1, false);

        var result = LegacyCatalogImportService.ExpandTariff(tariff, []);

        Assert.Collection(result,
            band => Assert.Equal((0, 2, 0.35m), (band.MinimumGrams, band.MaximumGrams, band.Price)),
            band => Assert.Equal((2, 4, 0.55m), (band.MinimumGrams, band.MaximumGrams, band.Price)),
            band => Assert.Equal((4, 6, 0.75m), (band.MinimumGrams, band.MaximumGrams, band.Price)));
    }

    [Fact]
    public void ExpandTariff_UsesAdjustedRangeAndRate()
    {
        var tariff = new LegacyCatalogImportService.LegacyTariff(
            9, 1, 10, 0, 1, 0.001m, 2m, 0.020m, 2, 0.20m, 1.10m, 2, false);
        var adjustments = new[]
        {
            new LegacyCatalogImportService.LegacyAdjustment(9, 0.001m, 0.020m, 0.001m, 0.100m, 0.85m),
            new LegacyCatalogImportService.LegacyAdjustment(9, 0.021m, 0.040m, 0.101m, 0.120m, 1.00m)
        };

        var result = LegacyCatalogImportService.ExpandTariff(tariff, adjustments);

        Assert.Collection(result,
            band => Assert.Equal((0, 100, 0.85m), (band.MinimumGrams, band.MaximumGrams, band.Price)),
            band => Assert.Equal((100, 120, 1.00m), (band.MinimumGrams, band.MaximumGrams, band.Price)));
    }

    [Fact]
    public void ExpandTariff_RoundsUpToNextFiveCents()
    {
        var tariff = new LegacyCatalogImportService.LegacyTariff(
            2, 1, 11, 0, 1, 0.001m, 2m, 0.020m, 2, 0.41m, 0.32m, 2, true);

        var result = LegacyCatalogImportService.ExpandTariff(tariff, []);

        Assert.Equal(0.45m, result[0].Price);
        Assert.Equal(0.75m, result[1].Price);
    }
}
