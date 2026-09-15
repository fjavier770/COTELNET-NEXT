namespace CotelNet.Domain.Sales;

public sealed class SalePayment
{
    private SalePayment() { }
    public SalePayment(int paymentMethodId, decimal amount) { PaymentMethodId = paymentMethodId; Amount = amount; }
    public int Id { get; private set; }
    public int SaleId { get; private set; }
    public Sale Sale { get; private set; } = null!;
    public int PaymentMethodId { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; } = null!;
    public decimal Amount { get; private set; }
}
