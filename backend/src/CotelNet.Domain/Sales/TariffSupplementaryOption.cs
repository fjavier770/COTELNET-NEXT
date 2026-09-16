namespace CotelNet.Domain.Sales;

public sealed class TariffSupplementaryOption
{
    private TariffSupplementaryOption() { }
    public TariffSupplementaryOption(int legacyTariffId, int supplementaryServiceId, decimal priceAdjustment)
    { LegacyTariffId = legacyTariffId; SupplementaryServiceId = supplementaryServiceId; PriceAdjustment = priceAdjustment; }
    public int LegacyTariffId { get; private set; }
    public int SupplementaryServiceId { get; private set; }
    public SupplementaryService SupplementaryService { get; private set; } = null!;
    public decimal PriceAdjustment { get; private set; }
}
