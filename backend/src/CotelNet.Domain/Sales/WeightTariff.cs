using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class WeightTariff : Entity
{
    private WeightTariff() { }
    public WeightTariff(int postalServiceId, string destinationZone, int minimumWeightGrams, int maximumWeightGrams, decimal price, int? legacyTariffId = null)
    {
        PostalServiceId = postalServiceId; DestinationZone = destinationZone.Trim().ToUpperInvariant(); MinimumWeightGrams = minimumWeightGrams; MaximumWeightGrams = maximumWeightGrams; Price = price; LegacyTariffId = legacyTariffId; Active = true;
    }
    public int PostalServiceId { get; private set; }
    public PostalService PostalService { get; private set; } = null!;
    public string DestinationZone { get; private set; } = string.Empty;
    public int MinimumWeightGrams { get; private set; }
    public int MaximumWeightGrams { get; private set; }
    public decimal Price { get; private set; }
    public int? LegacyTariffId { get; private set; }
    public bool Active { get; private set; }

    public void Update(int postalServiceId, string destinationZone, int minimumWeightGrams, int maximumWeightGrams, decimal price, bool active, int? legacyTariffId = null)
    { PostalServiceId = postalServiceId; DestinationZone = destinationZone.Trim().ToUpperInvariant(); MinimumWeightGrams = minimumWeightGrams; MaximumWeightGrams = maximumWeightGrams; Price = price; Active = active; LegacyTariffId = legacyTariffId; MarkUpdated(); }
}
