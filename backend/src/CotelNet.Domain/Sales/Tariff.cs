using CotelNet.Domain.Common;

namespace CotelNet.Domain.Sales;

public sealed class Tariff : Entity
{
    private Tariff() { }
    public Tariff(string code, string description, decimal price, int? productId = null, int? postalServiceId = null)
    {
        if ((productId is null) == (postalServiceId is null)) throw new ArgumentException("La tarifa debe pertenecer a un producto o servicio.");
        Code = code.Trim().ToUpperInvariant(); Description = description.Trim(); Price = price; ProductId = productId; PostalServiceId = postalServiceId; Active = true;
    }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool Active { get; private set; }
    public int? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public int? PostalServiceId { get; private set; }
    public PostalService? PostalService { get; private set; }
}
