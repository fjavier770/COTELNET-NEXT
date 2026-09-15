namespace CotelNet.Domain.Sales;

public sealed class SaleLine
{
    private SaleLine() { }
    public SaleLine(int tariffId, string code, string description, int quantity, decimal unitPrice)
    { TariffId = tariffId; Code = code; Description = description; Quantity = quantity; UnitPrice = unitPrice; Total = quantity * unitPrice; }
    public int Id { get; private set; }
    public int SaleId { get; private set; }
    public int TariffId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }
}
