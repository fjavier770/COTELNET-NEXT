namespace CotelNet.Application.Sales;

public sealed record SalesCatalogDto(IReadOnlyList<TariffDto> Tariffs, IReadOnlyList<PaymentMethodDto> PaymentMethods, IReadOnlyList<TerminalOptionDto> Terminals);
public sealed record TariffDto(int Id, string Code, string Description, decimal Price, string Type);
public sealed record PaymentMethodDto(int Id, string Code, string Name, bool IsCash);
public sealed record TerminalOptionDto(int Id, string Code, string Name, int EstafetaId, string Estafeta);
public sealed record OpenCashRequest(int TerminalId, decimal OpeningAmount);
public sealed record CloseCashRequest(decimal DeclaredCash);
public sealed record CashSessionDto(int Id, bool IsOpen, int TerminalId, string Terminal, string Estafeta, decimal OpeningAmount, DateTime OpenedAtUtc, DateTime? ClosedAtUtc, decimal SalesTotal, decimal CashSales, decimal ExpectedCash, decimal? DeclaredCash, decimal? Difference, IReadOnlyList<PaymentSummaryDto> Payments);
public sealed record PaymentSummaryDto(string PaymentMethod, decimal Amount);
public sealed record CreateSaleRequest(IReadOnlyCollection<CreateSaleLineRequest> Lines, IReadOnlyCollection<CreateSalePaymentRequest> Payments);
public sealed record CreateSaleLineRequest(int TariffId, int Quantity);
public sealed record CreateSalePaymentRequest(int PaymentMethodId, decimal Amount);
public sealed record SaleDto(int Id, string InvoiceNumber, DateTime CreatedAtUtc, decimal Total, IReadOnlyList<SaleLineDto> Lines, IReadOnlyList<PaymentSummaryDto> Payments);
public sealed record SaleLineDto(string Code, string Description, int Quantity, decimal UnitPrice, decimal Total);

public interface ISalesService
{
    Task<SalesCatalogDto> GetCatalogAsync(int userId, CancellationToken cancellationToken);
    Task<CashSessionDto?> GetCurrentCashAsync(int userId, CancellationToken cancellationToken);
    Task<CashSessionDto> OpenCashAsync(int userId, OpenCashRequest request, CancellationToken cancellationToken);
    Task<CashSessionDto> CloseCashAsync(int userId, CloseCashRequest request, CancellationToken cancellationToken);
    Task<SaleDto> CreateSaleAsync(int userId, CreateSaleRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<SaleDto>> GetRecentSalesAsync(int userId, CancellationToken cancellationToken);
}
