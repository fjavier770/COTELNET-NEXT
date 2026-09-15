using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class PaymentMethod : Entity
{
    private PaymentMethod() { }
    public PaymentMethod(string code, string name, bool isCash) { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); IsCash = isCash; Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsCash { get; private set; }
    public bool Active { get; private set; }
}
