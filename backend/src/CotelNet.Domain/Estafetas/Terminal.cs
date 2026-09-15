using CotelNet.Domain.Common;

namespace CotelNet.Domain.Estafetas;

public sealed class Terminal : Entity
{
    private Terminal() { }

    public Terminal(string code, string name, string? macAddress, int estafetaId)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        MacAddress = string.IsNullOrWhiteSpace(macAddress) ? null : macAddress.Trim().ToUpperInvariant();
        EstafetaId = estafetaId;
        Active = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? MacAddress { get; private set; }
    public bool Active { get; private set; }
    public int EstafetaId { get; private set; }
    public Estafeta Estafeta { get; private set; } = null!;

    public void Update(string code, string name, string? macAddress, int estafetaId, bool active)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        MacAddress = string.IsNullOrWhiteSpace(macAddress) ? null : macAddress.Trim().ToUpperInvariant();
        EstafetaId = estafetaId;
        Active = active;
        MarkUpdated();
    }
}

