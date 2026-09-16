using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class PostalService : Entity
{
    private PostalService() { }
    public PostalService(string code, string name, string s10Prefix)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        S10Prefix = NormalizeS10Prefix(s10Prefix);
        Active = true;
    }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string S10Prefix { get; private set; } = string.Empty;
    public bool Active { get; private set; }

    public void Update(string code, string name, string s10Prefix, bool active)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); S10Prefix = NormalizeS10Prefix(s10Prefix); Active = active; MarkUpdated(); }

    private static string NormalizeS10Prefix(string value)
    {
        var prefix = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (prefix.Length != 2 || prefix.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("El prefijo S10 debe contener exactamente dos letras.", nameof(value));
        return prefix;
    }
}
