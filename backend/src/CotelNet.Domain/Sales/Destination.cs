using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Destination : Entity
{
    private Destination() { }
    public Destination(string code, string name, string zone, bool isDomestic, int? legacyCountryId = null, int? legacyProvinceId = null)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Zone = zone.Trim().ToUpperInvariant(); IsDomestic = isDomestic; LegacyCountryId = legacyCountryId; LegacyProvinceId = legacyProvinceId; Active = true; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Zone { get; private set; } = string.Empty;
    public bool IsDomestic { get; private set; }
    public int? LegacyCountryId { get; private set; }
    public int? LegacyProvinceId { get; private set; }
    public bool Active { get; private set; }

    public void Update(string code, string name, string zone, bool isDomestic, bool active, int? legacyCountryId = null, int? legacyProvinceId = null)
    { Code = code.Trim().ToUpperInvariant(); Name = name.Trim(); Zone = zone.Trim().ToUpperInvariant(); IsDomestic = isDomestic; Active = active; LegacyCountryId = legacyCountryId; LegacyProvinceId = legacyProvinceId; MarkUpdated(); }
}
