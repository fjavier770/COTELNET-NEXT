using CotelNet.Application.Sales;
using CotelNet.Domain.Sales;
using CotelNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

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
        var postalServices = await db.PostalServices.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new PostalServiceDto(x.Id, x.Code, x.Name, x.S10Prefix)).ToListAsync(cancellationToken);
        var destinations = await db.Destinations.AsNoTracking().Where(x => x.Active).OrderByDescending(x => x.IsDomestic).ThenBy(x => x.Name).Select(x => new DestinationDto(x.Id, x.Code, x.Name, x.Zone, x.IsDomestic)).ToListAsync(cancellationToken);
        var supplementary = await db.SupplementaryServices.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).Select(x => new SupplementaryServiceDto(x.Id, x.Code, x.Name, x.Price)).ToListAsync(cancellationToken);
        var limits = await db.WeightTariffs.AsNoTracking().Where(x => x.Active)
            .GroupBy(x => new { x.PostalServiceId, x.DestinationZone })
            .Select(x => new ServiceWeightLimitDto(x.Key.PostalServiceId, x.Key.DestinationZone, x.Max(item => item.MaximumWeightGrams)))
            .ToListAsync(cancellationToken);
        return new SalesCatalogDto(tariffs, methods, terminals, postalServices, destinations, supplementary, limits);
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

    public async Task<ShipmentQuoteDto> QuoteShipmentAsync(QuoteShipmentRequest request, CancellationToken cancellationToken)
    {
        var (tariff, supplementary, available) = await GetShipmentPricesAsync(request.PostalServiceId, request.DestinationId, request.WeightGrams, request.SupplementaryServiceIds, cancellationToken);
        var supplementaryTotal = supplementary.Sum(x => x.Price);
        return new ShipmentQuoteDto(tariff.Price, supplementaryTotal, tariff.Price + supplementaryTotal, $"{tariff.MinimumWeightGrams + 1}–{tariff.MaximumWeightGrams} g", available.Select(ToDto).ToList());
    }

    public async Task<SenderProfileDto?> GetSenderProfileAsync(string document, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(document)) return null;
        var key = SenderProfile.NormalizeDocument(document);
        var sender = await db.SenderProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.DocumentKey == key, cancellationToken);
        return sender is null ? null : ToDto(sender);
    }

    public async Task<SaleDto> CreateShipmentAsync(int userId, CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        Require(request.SenderDocument, "La cédula o pasaporte del remitente"); Require(request.SenderFirstName, "El primer nombre del remitente"); Require(request.SenderFirstLastName, "El primer apellido del remitente"); Require(request.SenderAddress, "La dirección del remitente"); Require(request.RecipientName, "El nombre del destinatario"); Require(request.RecipientAddress, "La dirección del destinatario");
        var session = await db.CashSessions.SingleOrDefaultAsync(x => x.UserId == userId && x.IsOpen, cancellationToken) ?? throw new InvalidOperationException("Debes abrir la caja antes de registrar el envío.");
        var terminal = await db.Terminals.AsNoTracking().SingleAsync(x => x.Id == session.TerminalId, cancellationToken);
        var (tariff, supplementary, _) = await GetShipmentPricesAsync(request.PostalServiceId, request.DestinationId, request.WeightGrams, request.SupplementaryServiceIds, cancellationToken);
        var service = await db.PostalServices.AsNoTracking().SingleAsync(x => x.Id == request.PostalServiceId, cancellationToken);
        var destination = await db.Destinations.AsNoTracking().SingleAsync(x => x.Id == request.DestinationId, cancellationToken);
        var paymentMethod = await db.PaymentMethods.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.Active, cancellationToken) ?? throw new InvalidOperationException("La forma de pago no está disponible.");
        var total = tariff.Price + supplementary.Sum(x => x.Price);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var documentKey = SenderProfile.NormalizeDocument(request.SenderDocument);
        var sender = await db.SenderProfiles.SingleOrDefaultAsync(x => x.DocumentKey == documentKey, cancellationToken);
        if (sender is null)
        {
            sender = new SenderProfile(request.SenderDocument, request.SenderTitle, request.SenderIsMinor, request.SenderCountryCode, request.SenderFirstName!, request.SenderMiddleName,
                request.SenderFirstLastName!, request.SenderSecondLastName, request.SenderPhone, request.SenderSecondaryPhone, request.SenderEmail,
                request.SenderProvince, request.SenderCity, request.SenderPostalCode, request.SenderStreet, request.SenderHouseNumber, request.SenderAddress, request.SenderFax);
            db.SenderProfiles.Add(sender);
        }
        else
        {
            sender.Update(request.SenderDocument, request.SenderTitle, request.SenderIsMinor, request.SenderCountryCode, request.SenderFirstName!, request.SenderMiddleName,
                request.SenderFirstLastName!, request.SenderSecondLastName, request.SenderPhone, request.SenderSecondaryPhone, request.SenderEmail,
                request.SenderProvince, request.SenderCity, request.SenderPostalCode, request.SenderStreet, request.SenderHouseNumber, request.SenderAddress, request.SenderFax);
        }
        await db.SaveChangesAsync(cancellationToken);
        var sale = new Sale($"F-{DateTime.UtcNow:yyyyMMddHHmmssfff}", userId, terminal.EstafetaId, session.Id, total);
        sale.Lines.Add(new SaleLine(null, service.Code, $"{service.Name} · {destination.Name} · {request.WeightGrams} g", 1, tariff.Price));
        foreach (var item in supplementary) sale.Lines.Add(new SaleLine(null, item.Service.Code, item.Service.Name, 1, item.Price));
        sale.Payments.Add(new SalePayment(paymentMethod.Id, total));
        db.Sales.Add(sale); await db.SaveChangesAsync(cancellationToken);
        var estafeta = await db.Estafetas.AsNoTracking().SingleAsync(x => x.Id == terminal.EstafetaId, cancellationToken);
        var prefix = service.S10Prefix ?? supplementary.Select(x => x.Service.S10Prefix).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (string.IsNullOrWhiteSpace(prefix)) throw new InvalidOperationException("El servicio seleccionado no tiene un formato S10 vigente. Revise el tipo de código de envío en COTELNET.");
        var trackingNumber = await GenerateS10Async(prefix, estafeta.Codigo, cancellationToken);
        var shipment = new Shipment(sale.Id, trackingNumber, service.Id, destination.Id, request.WeightGrams, tariff.Price,
            sender.FullName, sender.DocumentNumber, sender.PrimaryPhone, sender.Email, sender.Address,
            request.RecipientName, request.RecipientPhone ?? string.Empty, request.RecipientAddress, sender.Id,
            request.RecipientTitle, request.RecipientFirstName, request.RecipientMiddleName, request.RecipientFirstLastName, request.RecipientSecondLastName,
            request.RecipientSecondaryPhone, request.RecipientEmail, request.RecipientProvince, request.RecipientCity,
            request.RecipientPostalCode, request.RecipientStreet, request.RecipientHouseNumber, request.RecipientFax);
        foreach (var item in supplementary) shipment.SupplementaryServices.Add(new ShipmentSupplementaryService(item.Service.Id, item.Service.Name, item.Price));
        db.Shipments.Add(shipment); await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        return await GetSaleDtoAsync(sale.Id, cancellationToken);
    }

    public async Task<SaleDto?> GetSaleAsync(int userId, int saleId, CancellationToken cancellationToken)
    {
        if (!await db.Sales.AnyAsync(x => x.Id == saleId && x.UserId == userId, cancellationToken)) return null;
        return await GetSaleDtoAsync(saleId, cancellationToken);
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
        var sale = await db.Sales.Include(x => x.Lines).Include(x => x.Payments).ThenInclude(x => x.PaymentMethod)
            .Include(x => x.Shipment)!.ThenInclude(x => x!.PostalService).Include(x => x.Shipment)!.ThenInclude(x => x!.Destination)
            .Include(x => x.Shipment)!.ThenInclude(x => x!.SupplementaryServices).AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
        ShipmentDto? shipment = null;
        if (sale.Shipment is not null)
            shipment = new ShipmentDto(sale.Shipment.TrackingNumber, S10CodeGenerator.ResolvePostalFormCode(sale.Shipment.TrackingNumber), sale.Shipment.WeightGrams, sale.Shipment.BasePrice, sale.Shipment.PostalService.Name, sale.Shipment.Destination.Name,
                sale.Shipment.SenderName, sale.Shipment.SenderDocument, sale.Shipment.SenderPhone, sale.Shipment.SenderEmail, sale.Shipment.SenderAddress,
                sale.Shipment.RecipientName, sale.Shipment.RecipientTitle, sale.Shipment.RecipientFirstName, sale.Shipment.RecipientMiddleName, sale.Shipment.RecipientFirstLastName, sale.Shipment.RecipientSecondLastName,
                sale.Shipment.RecipientPhone, sale.Shipment.RecipientSecondaryPhone, sale.Shipment.RecipientEmail,
                sale.Shipment.RecipientProvince, sale.Shipment.RecipientCity, sale.Shipment.RecipientPostalCode, sale.Shipment.RecipientStreet, sale.Shipment.RecipientHouseNumber, sale.Shipment.RecipientAddress, sale.Shipment.RecipientFax,
                sale.Shipment.SupplementaryServices.Select(x => new SupplementaryServiceDto(x.SupplementaryServiceId, string.Empty, x.Description, x.Price)).ToList(), CreateCode128Svg(sale.Shipment.TrackingNumber));
        var receipt = await (from user in db.Users.AsNoTracking()
                             join office in db.Estafetas.AsNoTracking() on sale.EstafetaId equals office.Id
                             join cash in db.CashSessions.AsNoTracking() on sale.CashSessionId equals cash.Id
                             join terminal in db.Terminals.AsNoTracking() on cash.TerminalId equals terminal.Id
                             where user.Id == sale.UserId
                             select new ReceiptContextDto(user.FullName, office.Codigo, office.Nombre, terminal.Code))
            .SingleAsync(cancellationToken);
        return new SaleDto(sale.Id, sale.InvoiceNumber, sale.CreatedAtUtc, sale.Total,
            sale.Lines.Select(x => new SaleLineDto(x.Code, x.Description, x.Quantity, x.UnitPrice, x.Total)).ToList(),
            sale.Payments.Select(x => new PaymentSummaryDto(x.PaymentMethod.Name, x.Amount)).ToList(), shipment, receipt);
    }

    private async Task<CashSessionDto> ToCashDtoAsync(CashSession session, CancellationToken cancellationToken)
    {
        var terminal = await db.Terminals.Include(x => x.Estafeta).AsNoTracking().SingleAsync(x => x.Id == session.TerminalId, cancellationToken);
        var payments = await db.SalePayments.Where(x => x.Sale.CashSessionId == session.Id).GroupBy(x => new { x.PaymentMethod.Name, x.PaymentMethod.IsCash })
            .Select(x => new { x.Key.Name, x.Key.IsCash, Amount = x.Sum(p => p.Amount) }).ToListAsync(cancellationToken);
        var salesTotal = payments.Sum(x => x.Amount); var cashSales = payments.Where(x => x.IsCash).Sum(x => x.Amount);
        return new CashSessionDto(session.Id, session.IsOpen, terminal.Id, $"{terminal.Code} - {terminal.Name}", terminal.Estafeta.Nombre, session.OpeningAmount, session.OpenedAtUtc, session.ClosedAtUtc, salesTotal, cashSales, session.OpeningAmount + cashSales, session.DeclaredCash, session.Difference, payments.Select(x => new PaymentSummaryDto(x.Name, x.Amount)).ToList());
    }

    private async Task<(WeightTariff Tariff, List<PricedSupplementary> Supplementary, List<PricedSupplementary> Available)> GetShipmentPricesAsync(int postalServiceId, int destinationId, int weightGrams, IReadOnlyCollection<int>? supplementaryIds, CancellationToken cancellationToken)
    {
        if (weightGrams <= 0) throw new InvalidOperationException("El peso debe ser mayor que cero.");
        var destination = await db.Destinations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == destinationId && x.Active, cancellationToken) ?? throw new InvalidOperationException("El destino no está disponible.");
        var serviceTariffs = db.WeightTariffs.AsNoTracking().Where(x => x.PostalServiceId == postalServiceId && x.DestinationZone == destination.Zone && x.Active);
        var maximumWeight = await serviceTariffs.MaxAsync(x => (int?)x.MaximumWeightGrams, cancellationToken);
        if (maximumWeight is null) throw new InvalidOperationException("No existe una tarifa para el servicio y destino seleccionados.");
        if (weightGrams > maximumWeight) throw new InvalidOperationException($"El peso máximo permitido para este servicio y destino es {maximumWeight.Value / 1000m:0.000} kg.");
        var tariff = await serviceTariffs.Where(x => weightGrams > x.MinimumWeightGrams && weightGrams <= x.MaximumWeightGrams).OrderBy(x => x.MaximumWeightGrams).FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No existe una tarifa para el rango de peso seleccionado.");
        List<PricedSupplementary> available;
        if (tariff.LegacyTariffId is int legacyTariffId)
        {
            var options = await db.TariffSupplementaryOptions.Include(x => x.SupplementaryService).AsNoTracking()
                .Where(x => x.LegacyTariffId == legacyTariffId && x.SupplementaryService.Active)
                .OrderBy(x => x.SupplementaryService.Name)
                .ToListAsync(cancellationToken);
            available = options.Select(x => new PricedSupplementary(x.SupplementaryService, x.PriceAdjustment > 0 ? x.PriceAdjustment : x.SupplementaryService.Price)).ToList();
        }
        else
        {
            var services = await db.SupplementaryServices.AsNoTracking().Where(x => x.Active).OrderBy(x => x.Name).ToListAsync(cancellationToken);
            available = services.Select(x => new PricedSupplementary(x, x.Price)).ToList();
        }
        var ids = (supplementaryIds ?? []).Distinct().ToArray();
        var supplementary = available.Where(x => ids.Contains(x.Service.Id)).ToList();
        if (supplementary.Count != ids.Length) throw new InvalidOperationException("Uno de los servicios suplementarios no aplica a la tarifa seleccionada.");
        return (tariff, supplementary, available);
    }

    private static SupplementaryServiceDto ToDto(PricedSupplementary x) => new(x.Service.Id, x.Service.Code, x.Service.Name, x.Price);
    private static SenderProfileDto ToDto(SenderProfile x) => new(x.DocumentNumber, x.CountryCode, x.Title, x.IsMinor, x.FirstName, x.MiddleName, x.FirstLastName, x.SecondLastName,
        x.PrimaryPhone, x.SecondaryPhone, x.Email, x.Province, x.City, x.PostalCode, x.Street, x.HouseNumber, x.Address, x.Fax);
    private static void Require(string? value, string field) { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"{field} es obligatorio."); }
    private async Task<string> GenerateS10Async(string prefix, string officeCode, CancellationToken cancellationToken)
    {
        var office = new string((officeCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (office.Length > 4) office = office[^4..];
        office = office.PadLeft(4, '0');
        var start = prefix.Trim().ToUpperInvariant() + office;
        var existing = await db.Shipments.AsNoTracking().Where(x => x.TrackingNumber.StartsWith(start) && x.TrackingNumber.EndsWith("PA"))
            .Select(x => x.TrackingNumber).ToListAsync(cancellationToken);
        var last = existing.Select(value => value.Length == 13 && int.TryParse(value.Substring(6, 4), out var number) ? number : 0).DefaultIfEmpty().Max();
        if (last >= 9999) throw new InvalidOperationException($"Se agotó el rango anual de códigos S10 para la estafeta {office} y el prefijo {prefix}.");
        return S10CodeGenerator.Generate(prefix, office, last + 1);
    }

    private sealed record PricedSupplementary(SupplementaryService Service, decimal Price);

    private static string CreateLegacyCode39Svg(string value)
    {
        const int narrow = 2;
        const int wide = 5;
        const int gap = 2;
        const int height = 72;
        const int quietZone = narrow * 10;
        var patterns = new Dictionary<char, string>
        {
            ['0']="nnnwwnwnn", ['1']="wnnwnnnnw", ['2']="nnwwnnnnw", ['3']="wnwwnnnnn", ['4']="nnnwwnnnw",
            ['5']="wnnwwnnnn", ['6']="nnwwwnnnn", ['7']="nnnwnnwnw", ['8']="wnnwnnwnn", ['9']="nnwwnnwnn",
            ['A']="wnnnnwnnw", ['B']="nnwnnwnnw", ['C']="wnwnnwnnn", ['D']="nnnnwwnnw", ['E']="wnnnwwnnn",
            ['F']="nnwnwwnnn", ['G']="nnnnnwwnw", ['H']="wnnnnwwnn", ['I']="nnwnnwwnn", ['J']="nnnnwwwnn",
            ['K']="wnnnnnnww", ['L']="nnwnnnnww", ['M']="wnwnnnnwn", ['N']="nnnnwnnww", ['O']="wnnnwnnwn",
            ['P']="nnwnwnnwn", ['Q']="nnnnnnwww", ['R']="wnnnnnwwn", ['S']="nnwnnnwwn", ['T']="nnnnwnwwn",
            ['U']="wwnnnnnnw", ['V']="nwwnnnnnw", ['W']="wwwnnnnnn", ['X']="nwnnwnnnw", ['Y']="wwnnwnnnn",
            ['Z']="nwwnwnnnn", ['-']="nwnnnnwnw", ['.']="wwnnnnwnn", [' ']="nwwnnnwnn", ['$']="nwnwnwnnn",
            ['/']="nwnwnnnwn", ['+']="nwnnnwnwn", ['%']="nnnwnwnwn", ['*']="nwnnwnwnn"
        };
        var encoded = $"*{value.Trim().ToUpperInvariant()}*";
        var x = quietZone;
        var bars = new StringBuilder();
        foreach (var character in encoded)
        {
            if (!patterns.TryGetValue(character, out var pattern)) throw new InvalidOperationException("El código contiene caracteres no válidos para Code 39.");
            for (var index = 0; index < pattern.Length; index++)
            {
                var width = pattern[index] == 'w' ? wide : narrow;
                if (index % 2 == 0) bars.Append($"<rect x=\"{x}\" y=\"4\" width=\"{width}\" height=\"{height}\"/>");
                x += width;
            }
            x += gap;
        }
        var widthTotal = x + quietZone;
        S10CodeGenerator.TryFormatHumanReadable(value, out var humanReadable);
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {widthTotal} 100\" role=\"img\" aria-label=\"{humanReadable}\"><rect width=\"100%\" height=\"100%\" fill=\"white\"/>{bars}<text x=\"{widthTotal / 2}\" y=\"94\" font-family=\"Arial, sans-serif\" font-size=\"13\" text-anchor=\"middle\">{humanReadable}</text></svg>";
    }

    private static string CreateCode128Svg(string value)
    {
        const int moduleWidth = 2;
        const int quietZone = moduleWidth * 10;
        const int barHeight = 80;
        const int svgHeight = 112;
        var patterns = new[]
        {
            "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213", "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132", "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211", "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313", "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331", "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111", "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214", "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111", "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141", "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141", "114131", "311141", "411131", "211412", "211214", "211232", "2331112"
        };
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Any(character => character < ' ' || character > '~'))
            throw new InvalidOperationException("El código contiene caracteres no compatibles con Code 128.");

        var codes = new List<int> { 104 };
        codes.AddRange(normalized.Select(character => character - 32));
        var checksum = codes.Select((code, index) => index == 0 ? code : code * index).Sum() % 103;
        codes.Add(checksum);
        codes.Add(106);

        var x = quietZone;
        var bars = new StringBuilder();
        foreach (var code in codes)
        {
            var pattern = patterns[code];
            for (var index = 0; index < pattern.Length; index++)
            {
                var width = (pattern[index] - '0') * moduleWidth;
                if (index % 2 == 0) bars.Append($"<rect x=\"{x}\" y=\"4\" width=\"{width}\" height=\"{barHeight}\"/>");
                x += width;
            }
        }

        var widthTotal = x + quietZone;
        S10CodeGenerator.TryFormatHumanReadable(value, out var humanReadable);
        return $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {widthTotal} {svgHeight}\" role=\"img\" aria-label=\"{humanReadable}\"><rect width=\"100%\" height=\"100%\" fill=\"white\"/>{bars}<text x=\"{widthTotal / 2}\" y=\"106\" font-family=\"Arial, sans-serif\" font-size=\"18\" font-weight=\"700\" text-anchor=\"middle\">{humanReadable}</text></svg>";
    }
}
