using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Sale : Entity
{
    private Sale() { }
    public Sale(string invoiceNumber, int userId, int estafetaId, int cashSessionId, decimal total)
    { InvoiceNumber = invoiceNumber; UserId = userId; EstafetaId = estafetaId; CashSessionId = cashSessionId; Total = total; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public int EstafetaId { get; private set; }
    public int CashSessionId { get; private set; }
    public decimal Total { get; private set; }
    public ICollection<SaleLine> Lines { get; private set; } = new List<SaleLine>();
    public ICollection<SalePayment> Payments { get; private set; } = new List<SalePayment>();
    public Shipment? Shipment { get; private set; }
}
