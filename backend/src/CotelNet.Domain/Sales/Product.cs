using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Product : Entity
{
    private Product() { }
    public Product(string code, string name) { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool Active { get; private set; }
}
