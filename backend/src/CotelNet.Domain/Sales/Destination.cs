using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Destination : Entity
{
    private Destination() { }
    public Destination(string code, string name, string zone, bool isDomestic)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Zone = zone.Trim().ToUpperInvariant(); IsDomestic = isDomestic; Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Zone { get; private set; } = string.Empty;
    public bool IsDomestic { get; private set; }
    public bool Active { get; private set; }

    public void Update(string code, string name, string zone, bool isDomestic, bool active)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Zone = zone.Trim().ToUpperInvariant(); IsDomestic = isDomestic; Active = active; MarkUpdated(); }
}
