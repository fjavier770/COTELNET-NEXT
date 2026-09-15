using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class CashSession : Entity
{
    private CashSession() { }
    public CashSession(int userId, int terminalId, decimal openingAmount)
    {
        if (openingAmount < 0) throw new ArgumentException("El fondo inicial no puede ser negativo.");
        UserId = userId; TerminalId = terminalId; OpeningAmount = openingAmount; OpenedAtUtc = DateTime.UtcNow; IsOpen = true;
    }
    public int UserId { get; private set; }
    public int TerminalId { get; private set; }
    public decimal OpeningAmount { get; private set; }
    public DateTime OpenedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public decimal? ExpectedCash { get; private set; }
    public decimal? DeclaredCash { get; private set; }
    public decimal? Difference { get; private set; }
    public bool IsOpen { get; private set; }

    public void Close(decimal expectedCash, decimal declaredCash)
    {
        if (!IsOpen) throw new InvalidOperationException("La caja ya está cerrada.");
        if (declaredCash < 0) throw new ArgumentException("El efectivo declarado no puede ser negativo.");
        ExpectedCash = expectedCash; DeclaredCash = declaredCash; Difference = declaredCash - expectedCash; ClosedAtUtc = DateTime.UtcNow; IsOpen = false; MarkUpdated();
    }
}
