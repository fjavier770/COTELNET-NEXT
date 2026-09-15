namespace CotelNet.Domain.Sales;

public sealed class ShipmentSupplementaryService
{
    private ShipmentSupplementaryService() { }
    public ShipmentSupplementaryService(int supplementaryServiceId, string description, decimal price)
    { SupplementaryServiceId = supplementaryServiceId; Description = description; Price = price; }
    public int Id { get; private set; }
    public int ShipmentId { get; private set; }
    public int SupplementaryServiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
}
