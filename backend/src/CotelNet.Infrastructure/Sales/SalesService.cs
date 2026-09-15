using CotelNet.Application.Sales;
using CotelNet.Domain.Sales;
using CotelNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Infrastructure.Sales;

public sealed class SalesService(CotelNetDbContext db) : ISalesService
{
    public async Task<SalesCatalogDto> GetCatalogAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == userId, cancellationToken);
        var tariffs = await db.Tariffs.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Description)
            .Select(x => new TariffDto(x.Id, x.Code, x.Description, x.Price, x.ProductId != null ? "Producto" : "Servicio postal")).ToListAsync(cancellationToken);
        var methods = await db.PaymentMethods.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Id)
            .Select(x => new PaymentMethodDto(x.Id, x.Code, x.Name, x.IsCash)).ToListAsync(cancellationToken);
        var terminalsQuery = db.Terminals.Include(x => x.Estafeta).AsNoTracking().Where(x => x.Active && x.Estafeta.Activa);
        if (user.EstafetaId is int estafetaId) terminalsQuery = terminalsQuery.Where(x => x.EstafetaId == estafetaId);
        var terminals = await terminalsQuery.OrderBy(x => x.Code)
            .Select(x => new TerminalOptionDto(x.Id, x.Code, x.Name, x.EstafetaId, x.Estafeta.Nombre)).ToListAsync(cancellationToken);
        return new SalesCatalogDto(tariffs, methods, terminals);
    }

    public async Task<CashSessionDto?> GetCurrentCashAsync(int userId, CancellationToken cancellationToken)
    {
        var session = await db.CashSessions.AsNoTracking().Where(x => x.UserId == userId && x.IsOpen).OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        return session is null ? null : await ToCashDtoAsync(session, cancellationToken);
    }

    public async Task<CashSessionDto> OpenCashAsync(int userId, OpenCashRequest request, CancellationToken cancellationToken)
    {
        if (await db.CashSessions.AnyAsync(x => x.UserId == userId && x.IsOpen, cancellationToken)) throw new InvalidOperationException("Ya tienes una caja abierta.");
        if (await db.CashSessions.AnyAsync(x => x.TerminalId == request.TerminalId && x.IsOpen, cancellationToken)) throw new InvalidOperationException("La terminal seleccionada ya tiene una caja abierta.");
        var user = await db.Users.AsNoTracking().SingleAsync(x => x.Id == userId, cancellationToken);
        var terminal = await db.Terminals.Include(x => x.Estafeta).SingleOrDefaultAsync(x => x.Id == request.TerminalId && x.Active, cancellationToken) ?? throw new InvalidOperationException("La terminal no está disponible.");
        if (user.EstafetaId is int estafetaId && terminal.EstafetaId != estafetaId) throw new InvalidOperationException("La terminal no pertenece a tu estafeta.");
        var session = new CashSession(userId, terminal.Id, request.OpeningAmount);
        db.CashSessions.Add(session); await db.SaveChangesAsync(cancellationToken);
        return await ToCashDtoAsync(session, cancellationToken);
    }

    public async Task<CashSessionDto> CloseCashAsync(int userId, CloseCashRequest request, CancellationToken cancellationToken)
    {
        var session = await db.CashSessions.SingleOrDefaultAsync(x => x.UserId == userId && x.IsOpen, cancellationToken) ?? throw new InvalidOperationException("No tienes una caja abierta.");
        var cashSales = await db.SalePayments.Where(x => x.Sale.CashSessionId == session.Id && x.PaymentMethod.IsCash).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        session.Close(session.OpeningAmount + cashSales, request.DeclaredCash); await db.SaveChangesAsync(cancellationToken);
        return await ToCashDtoAsync(session, cancellationToken);
    }

    public async Task<SaleDto> CreateSaleAsync(int userId, CreateSaleRequest request, CancellationToken cancellationToken)
    {
        if (request.Lines.Count == 0) throw new InvalidOperationException("Agrega al menos un producto o servicio.");
        var session = await db.CashSessions.SingleOrDefaultAsync(x => x.UserId == userId && x.IsOpen, cancellationToken) ?? throw new InvalidOperationException("Debes abrir la caja antes de registrar ventas.");
        var terminal = await db.Terminals.AsNoTracking().SingleAsync(x => x.Id == session.TerminalId, cancellationToken);
        var tariffIds = request.Lines.Select(x => x.TariffId).Distinct().ToArray();
        var tariffs = await db.Tariffs.Where(x => tariffIds.Contains(x.Id) && x.Active).ToDictionaryAsync(x => x.Id, cancellationToken);
        if (tariffs.Count != tariffIds.Length) throw new InvalidOperationException("Una o más tarifas no están disponibles.");
        if (request.Lines.Any(x => x.Quantity <= 0)) throw new InvalidOperationException("Las cantidades deben ser mayores que cero.");
        var lineData = request.Lines.Select(x => new { Tariff = tariffs[x.TariffId], x.Quantity }).ToList();
        var total = lineData.Sum(x => x.Tariff.Price * x.Quantity);
        if (request.Payments.Count == 0 || request.Payments.Any(x => x.Amount <= 0) || request.Payments.Sum(x => x.Amount) != total) throw new InvalidOperationException("Los pagos deben coincidir exactamente con el total de la venta.");
        var methodIds = request.Payments.Select(x => x.PaymentMethodId).Distinct().ToArray();
        if (await db.PaymentMethods.CountAsync(x => methodIds.Contains(x.Id) && x.Active, cancellationToken) != methodIds.Length) throw new InvalidOperationException("Una forma de pago no está disponible.");
        var sale = new Sale($"V-{DateTime.UtcNow:yyyyMMddHHmmssfff}", userId, terminal.EstafetaId, session.Id, total);
        foreach (var line in lineData) sale.Lines.Add(new SaleLine(line.Tariff.Id, line.Tariff.Code, line.Tariff.Description, line.Quantity, line.Tariff.Price));
        foreach (var payment in request.Payments) sale.Payments.Add(new SalePayment(payment.PaymentMethodId, payment.Amount));
        db.Sales.Add(sale); await db.SaveChangesAsync(cancellationToken);
        return await GetSaleDtoAsync(sale.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<SaleDto>> GetRecentSalesAsync(int userId, CancellationToken cancellationToken)
    {
        var ids = await db.Sales.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.Id).Take(10).Select(x => x.Id).ToListAsync(cancellationToken);
        var result = new List<SaleDto>();
        foreach (var id in ids) result.Add(await GetSaleDtoAsync(id, cancellationToken));
        return result;
    }

    private async Task<SaleDto> GetSaleDtoAsync(int id, CancellationToken cancellationToken)
    {
        var sale = await db.Sales.Include(x => x.Lines).Include(x => x.Payments).ThenInclude(x => x.PaymentMethod).AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
        return new SaleDto(sale.Id, sale.InvoiceNumber, sale.CreatedAtUtc, sale.Total,
            sale.Lines.Select(x => new SaleLineDto(x.Code, x.Description, x.Quantity, x.UnitPrice, x.Total)).ToList(),
            sale.Payments.Select(x => new PaymentSummaryDto(x.PaymentMethod.Name, x.Amount)).ToList());
    }

    private async Task<CashSessionDto> ToCashDtoAsync(CashSession session, CancellationToken cancellationToken)
    {
        var terminal = await db.Terminals.Include(x => x.Estafeta).AsNoTracking().SingleAsync(x => x.Id == session.TerminalId, cancellationToken);
        var payments = await db.SalePayments.Where(x => x.Sale.CashSessionId == session.Id).GroupBy(x => new { x.PaymentMethod.Name, x.PaymentMethod.IsCash })
            .Select(x => new { x.Key.Name, x.Key.IsCash, Amount = x.Sum(p => p.Amount) }).ToListAsync(cancellationToken);
        var salesTotal = payments.Sum(x => x.Amount); var cashSales = payments.Where(x => x.IsCash).Sum(x => x.Amount);
        return new CashSessionDto(session.Id, session.IsOpen, terminal.Id, $"{terminal.Code} - {terminal.Name}", terminal.Estafeta.Nombre, session.OpeningAmount, session.OpenedAtUtc, session.ClosedAtUtc, salesTotal, cashSales, session.OpeningAmount + cashSales, session.DeclaredCash, session.Difference, payments.Select(x => new PaymentSummaryDto(x.Name, x.Amount)).ToList());
    }
}
