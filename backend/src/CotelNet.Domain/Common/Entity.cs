namespace CotelNet.Domain.Common;

public abstract class Entity
{
    public int Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected void MarkUpdated() => UpdatedAtUtc = DateTime.UtcNow;
}

