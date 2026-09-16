using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class SupplementaryService : Entity
{
    private SupplementaryService() { }
    public SupplementaryService(string code, string name, decimal price, int? legacyId = null, string? s10Prefix = null)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Price = price; LegacyId = legacyId; S10Prefix = NormalizePrefix(s10Prefix); Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int? LegacyId { get; private set; }
    public string? S10Prefix { get; private set; }
    public bool Active { get; private set; }

    public void Update(string code, string name, decimal price, bool active, int? legacyId = null, string? s10Prefix = null)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Price = price; Active = active; LegacyId = legacyId; S10Prefix = NormalizePrefix(s10Prefix); MarkUpdated(); }

    private static string? NormalizePrefix(string? value)
    {
        var prefix = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(prefix)) return null;
        if (prefix.Length != 2 || prefix.Any(character => character is < 'A' or > 'Z')) throw new ArgumentException("El prefijo S10 debe contener dos letras.", nameof(value));
        return prefix;
    }
}
