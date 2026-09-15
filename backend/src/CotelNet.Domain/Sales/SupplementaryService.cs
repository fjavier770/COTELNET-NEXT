using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class SupplementaryService : Entity
{
    private SupplementaryService() { }
    public SupplementaryService(string code, string name, decimal price)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Price = price; Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool Active { get; private set; }
}
