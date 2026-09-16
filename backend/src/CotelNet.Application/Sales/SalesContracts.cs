namespace CotelNet.Application.Sales;

public sealed record SalesCatalogDto(IReadOnlyList<TariffDto> Tariffs, IReadOnlyList<PaymentMethodDto> PaymentMethods, IReadOnlyList<TerminalOptionDto> Terminals, IReadOnlyList<PostalServiceDto> PostalServices, IReadOnlyList<DestinationDto> Destinations, IReadOnlyList<SupplementaryServiceDto> SupplementaryServices, IReadOnlyList<ServiceWeightLimitDto> ServiceWeightLimits);
public sealed record TariffDto(int Id, string Code, string Description, decimal Price, string Type);
public sealed record PostalServiceDto(int Id, string Code, string Name, string? S10Prefix);
public sealed record DestinationDto(int Id, string Code, string Name, string Zone, bool IsDomestic);
public sealed record ServiceWeightLimitDto(int PostalServiceId, string DestinationZone, int MaximumWeightGrams);
public sealed record SupplementaryServiceDto(int Id, string Code, string Name, decimal Price);
public sealed record PaymentMethodDto(int Id, string Code, string Name, bool IsCash);
public sealed record TerminalOptionDto(int Id, string Code, string Name, int EstafetaId, string Estafeta);
public sealed record OpenCashRequest(int TerminalId, decimal OpeningAmount);
public sealed record CloseCashRequest(decimal DeclaredCash);
public sealed record CashSessionDto(int Id, bool IsOpen, int TerminalId, string Terminal, string Estafeta, decimal OpeningAmount, DateTime OpenedAtUtc, DateTime? ClosedAtUtc, decimal SalesTotal, decimal CashSales, decimal ExpectedCash, decimal? DeclaredCash, decimal? Difference, IReadOnlyList<PaymentSummaryDto> Payments);
public sealed record PaymentSummaryDto(string PaymentMethod, decimal Amount);
public sealed record CreateSaleRequest(IReadOnlyCollection<CreateSaleLineRequest> Lines, IReadOnlyCollection<CreateSalePaymentRequest> Payments);
public sealed record CreateSaleLineRequest(int TariffId, int Quantity);
public sealed record CreateSalePaymentRequest(int PaymentMethodId, decimal Amount);
public sealed record SaleDto(int Id, string InvoiceNumber, DateTime CreatedAtUtc, decimal Total, IReadOnlyList<SaleLineDto> Lines, IReadOnlyList<PaymentSummaryDto> Payments, ShipmentDto? Shipment, ReceiptContextDto Receipt);
public sealed record ReceiptContextDto(string Cashier, string OfficeCode, string OfficeName, string TerminalCode);
public sealed record SaleLineDto(string Code, string Description, int Quantity, decimal UnitPrice, decimal Total);
public sealed record QuoteShipmentRequest(int PostalServiceId, int DestinationId, int WeightGrams, IReadOnlyCollection<int>? SupplementaryServiceIds);
public sealed record ShipmentQuoteDto(decimal BasePrice, decimal SupplementaryTotal, decimal Total, string WeightBand, IReadOnlyList<SupplementaryServiceDto> SupplementaryServices);
public sealed record CreateShipmentRequest(string SenderName, string SenderDocument, string SenderPhone, string SenderEmail, string SenderAddress, string RecipientName, string RecipientPhone, string RecipientAddress, int PostalServiceId, int DestinationId, int WeightGrams, IReadOnlyCollection<int>? SupplementaryServiceIds, int PaymentMethodId);
public sealed record ShipmentDto(string TrackingNumber, int WeightGrams, decimal BasePrice, string PostalService, string Destination, string SenderName, string SenderDocument, string SenderPhone, string SenderEmail, string SenderAddress, string RecipientName, string RecipientPhone, string RecipientAddress, IReadOnlyList<SupplementaryServiceDto> SupplementaryServices, string BarcodeSvg);

public interface ISalesService
{
    Task<SalesCatalogDto> GetCatalogAsync(int userId, CancellationToken cancellationToken);
    Task<CashSessionDto?> GetCurrentCashAsync(int userId, CancellationToken cancellationToken);
    Task<CashSessionDto> OpenCashAsync(int userId, OpenCashRequest request, CancellationToken cancellationToken);
    Task<CashSessionDto> CloseCashAsync(int userId, CloseCashRequest request, CancellationToken cancellationToken);
    Task<SaleDto> CreateSaleAsync(int userId, CreateSaleRequest request, CancellationToken cancellationToken);
    Task<ShipmentQuoteDto> QuoteShipmentAsync(QuoteShipmentRequest request, CancellationToken cancellationToken);
    Task<SaleDto> CreateShipmentAsync(int userId, CreateShipmentRequest request, CancellationToken cancellationToken);
    Task<SaleDto?> GetSaleAsync(int userId, int saleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SaleDto>> GetRecentSalesAsync(int userId, CancellationToken cancellationToken);
}
