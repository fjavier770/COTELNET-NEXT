using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class PostalService : Entity
{
    private PostalService() { }
    public PostalService(string code, string name) { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool Active { get; private set; }

    public void Update(string code, string name, bool active)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Active = active; MarkUpdated(); }
}
