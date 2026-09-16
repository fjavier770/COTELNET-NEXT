using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class PostalService : Entity
{
    private PostalService() { }
    public PostalService(string code, string name, string? s10Prefix, int? legacyServiceTypeId = null, int? legacyRouteId = null)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        S10Prefix = NormalizeS10Prefix(s10Prefix);
        LegacyServiceTypeId = legacyServiceTypeId;
        LegacyRouteId = legacyRouteId;
        Active = true;
    }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? S10Prefix { get; private set; }
    public int? LegacyServiceTypeId { get; private set; }
    public int? LegacyRouteId { get; private set; }
    public bool Active { get; private set; }

    public void Update(string code, string name, string? s10Prefix, bool active, int? legacyServiceTypeId = null, int? legacyRouteId = null)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); S10Prefix = NormalizeS10Prefix(s10Prefix); Active = active; LegacyServiceTypeId = legacyServiceTypeId; LegacyRouteId = legacyRouteId; MarkUpdated(); }

    private static string? NormalizeS10Prefix(string? value)
    {
        var prefix = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (prefix.Length == 0) return null;
        if (prefix.Length != 2 || prefix.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("El prefijo S10 debe contener exactamente dos letras.", nameof(value));
        return prefix;
    }
}
