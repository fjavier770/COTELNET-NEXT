using System.Data;
using CotelNet.Application.Administration;
using CotelNet.Domain.Sales;
using CotelNet.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CotelNet.Infrastructure.Administration;

public sealed class LegacyCatalogImportService(CotelNetDbContext db, IConfiguration configuration) : ILegacyCatalogImportService
{
    private string? ConnectionString => configuration.GetConnectionString("LegacyCotelNet");

    public Task<bool> IsConfiguredAsync(CancellationToken cancellationToken) => Task.FromResult(!string.IsNullOrWhiteSpace(ConnectionString));

    public async Task<LegacyCatalogImportResult> ImportAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("Configura ConnectionStrings__LegacyCotelNet para importar los catálogos.");

        var source = new SqlConnectionStringBuilder(ConnectionString) { ApplicationIntent = ApplicationIntent.ReadOnly };
        await using var legacy = new SqlConnection(source.ConnectionString);
        await legacy.OpenAsync(cancellationToken);

        var services = await ReadServicesAsync(legacy, cancellationToken);
        var countries = await ReadCountriesAsync(legacy, cancellationToken);
        var provinces = await ReadProvincesAsync(legacy, cancellationToken);
        var countryGroups = await ReadPairsAsync(legacy, "SELECT grupoID, paisID FROM [Grupos.Paises]", cancellationToken);
        var provinceGroups = await ReadPairsAsync(legacy, "SELECT grupoID, provinciaPanamaID FROM [Grupos.ProvinciasPanama]", cancellationToken);
        var tariffs = await ReadTariffsAsync(legacy, cancellationToken);
        var adjustments = await ReadAdjustmentsAsync(legacy, cancellationToken);
        var supplementals = await ReadSupplementalsAsync(legacy, cancellationToken);
        var supplementalLinks = await ReadSupplementalLinksAsync(legacy, cancellationToken);
        var codeTypes = await ReadCodeTypesAsync(legacy, cancellationToken);
        var prefixes = await ResolvePrefixesAsync(legacy, codeTypes.Select(x => x.Id).Distinct(), cancellationToken);

        var warnings = new List<string>();
        var activeServiceTypes = services.Select(x => x.ServiceTypeId).ToHashSet();
        var activeTariffs = tariffs.Where(x => activeServiceTypes.Contains(x.ServiceTypeId)).ToList();
        if (activeTariffs.Count == 0) throw new InvalidOperationException("No se encontraron tarifas activas para servicios postales operativos.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.TariffSupplementaryOptions.RemoveRange(db.TariffSupplementaryOptions);
        db.WeightTariffs.RemoveRange(db.WeightTariffs);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var current in await db.PostalServices.ToListAsync(cancellationToken))
            current.Update(current.Code, current.Name, current.S10Prefix, false, current.LegacyServiceTypeId, current.LegacyRouteId);
        foreach (var current in await db.Destinations.ToListAsync(cancellationToken))
            current.Update(current.Code, current.Name, current.Zone, current.IsDomestic, false, current.LegacyCountryId, current.LegacyProvinceId);
        foreach (var current in await db.SupplementaryServices.ToListAsync(cancellationToken))
            current.Update(current.Code, current.Name, current.Price, false, current.LegacyId, current.S10Prefix);

        var importedServices = new List<(LegacyService Source, PostalService Target)>();
        foreach (var sourceService in services)
        {
            var directCodeType = codeTypes.FirstOrDefault(x => x.ServiceTypeIds.Contains(sourceService.ServiceTypeId));
            prefixes.TryGetValue(directCodeType?.Id ?? 0, out var prefix);
            var code = $"ST{sourceService.ServiceTypeId}-V{sourceService.RouteId}";
            var name = $"{sourceService.CategoryName} · {sourceService.TypeName} · {sourceService.RouteName}";
            var target = await db.PostalServices.SingleOrDefaultAsync(x => x.LegacyServiceTypeId == sourceService.ServiceTypeId && x.LegacyRouteId == sourceService.RouteId, cancellationToken);
            if (target is null) { target = new PostalService(code, name, prefix, sourceService.ServiceTypeId, sourceService.RouteId); db.PostalServices.Add(target); }
            else target.Update(code, name, prefix, true, sourceService.ServiceTypeId, sourceService.RouteId);
            importedServices.Add((sourceService, target));
        }

        var destinations = new List<(LegacyDestination Source, Destination Target)>();
        foreach (var country in countries.Where(IsRealCountry).GroupBy(x => x.Iso2).Select(x => x.OrderBy(item => item.Id).First()))
        {
            var code = country.Iso2;
            var target = await db.Destinations.SingleOrDefaultAsync(x => x.LegacyCountryId == country.Id || (x.LegacyCountryId == null && x.Code == code), cancellationToken);
            if (target is null) { target = new Destination(code, country.Name, $"PAIS-{country.Id}", false, legacyCountryId: country.Id); db.Destinations.Add(target); }
            else target.Update(code, country.Name, $"PAIS-{country.Id}", false, true, legacyCountryId: country.Id);
            destinations.Add((new LegacyDestination(country.Id, false, country.Name, $"PAIS-{country.Id}"), target));
        }
        foreach (var province in provinces)
        {
            var code = $"PA-{province.Id:D2}";
            var target = await db.Destinations.SingleOrDefaultAsync(x => x.LegacyProvinceId == province.Id, cancellationToken);
            if (target is null) { target = new Destination(code, province.Name, $"PROV-{province.Id}", true, legacyProvinceId: province.Id); db.Destinations.Add(target); }
            else target.Update(code, province.Name, $"PROV-{province.Id}", true, true, legacyProvinceId: province.Id);
            destinations.Add((new LegacyDestination(province.Id, true, province.Name, $"PROV-{province.Id}"), target));
        }

        var referencedSupplementalIds = supplementalLinks.Where(x => activeTariffs.Any(t => t.Id == x.TariffId)).Select(x => x.SupplementaryId).ToHashSet();
        var importedSupplementals = new Dictionary<int, SupplementaryService>();
        foreach (var item in supplementals.Where(x => referencedSupplementalIds.Contains(x.Id)))
        {
            var codeType = codeTypes.FirstOrDefault(x => x.SupplementaryIds.Contains(item.Id));
            prefixes.TryGetValue(codeType?.Id ?? 0, out var prefix);
            var target = await db.SupplementaryServices.SingleOrDefaultAsync(x => x.LegacyId == item.Id, cancellationToken);
            if (target is null) { target = new SupplementaryService($"SS-{item.Id}", item.Name, item.Price, item.Id, prefix); db.SupplementaryServices.Add(target); }
            else target.Update($"SS-{item.Id}", item.Name, item.Price, true, item.Id, prefix);
            importedSupplementals[item.Id] = target;
        }
        await db.SaveChangesAsync(cancellationToken);

        var countryGroupLookup = countryGroups.GroupBy(x => x.Right).ToDictionary(x => x.Key, x => x.Select(pair => pair.Left).ToHashSet());
        var provinceGroupLookup = provinceGroups.GroupBy(x => x.Right).ToDictionary(x => x.Key, x => x.Select(pair => pair.Left).ToHashSet());
        var adjustmentsLookup = adjustments.GroupBy(x => x.TariffId).ToDictionary(x => x.Key, x => x.ToList());
        var weightBands = 0;

        foreach (var service in importedServices)
        foreach (var destination in destinations)
        {
            var candidates = activeTariffs.Where(x => x.ServiceTypeId == service.Source.ServiceTypeId && x.RouteId == service.Source.RouteId).ToList();
            LegacyTariff? tariff;
            if (destination.Source.IsDomestic)
            {
                var groups = provinceGroupLookup.GetValueOrDefault(destination.Source.LegacyId) ?? [];
                tariff = candidates.Where(x => x.ScopeId == 1 && groups.Contains(x.GroupId)).OrderBy(x => x.Id).FirstOrDefault();
            }
            else
            {
                tariff = candidates.Where(x => x.CountryId == destination.Source.LegacyId).OrderBy(x => x.Id).FirstOrDefault();
                if (tariff is null)
                {
                    var groups = countryGroupLookup.GetValueOrDefault(destination.Source.LegacyId) ?? [];
                    tariff = candidates.Where(x => x.ScopeId == 2 && groups.Contains(x.GroupId)).OrderBy(x => x.Id).FirstOrDefault();
                }
            }
            if (tariff is null) continue;
            foreach (var band in ExpandTariff(tariff, adjustmentsLookup.GetValueOrDefault(tariff.Id) ?? []))
            {
                db.WeightTariffs.Add(new WeightTariff(service.Target.Id, destination.Target.Zone, band.MinimumGrams, band.MaximumGrams, band.Price, tariff.Id));
                weightBands++;
            }
        }

        var activeTariffIds = activeTariffs.Select(x => x.Id).ToHashSet();
        var optionCount = 0;
        foreach (var link in supplementalLinks.Where(x => activeTariffIds.Contains(x.TariffId)))
            if (importedSupplementals.TryGetValue(link.SupplementaryId, out var item))
            {
                db.TariffSupplementaryOptions.Add(new TariffSupplementaryOption(link.TariffId, item.Id, link.PriceAdjustment));
                optionCount++;
            }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        if (prefixes.Count == 0) warnings.Add("No se pudieron resolver formatos S10 vigentes; revise el año configurado y el procedimiento legado.");
        return new LegacyCatalogImportResult(importedServices.Count, destinations.Count, weightBands, importedSupplementals.Count, optionCount, warnings);
    }

    private static bool IsRealCountry(LegacyCountry country) => country.Id != 170 && country.Id < 247 && country.Id != 242 && country.Iso2.Length == 2 && country.Iso2.All(char.IsLetter);

    internal static IReadOnlyList<ExpandedBand> ExpandTariff(LegacyTariff tariff, IReadOnlyList<LegacyAdjustment> adjustments)
    {
        var result = new List<ExpandedBand>();
        decimal start = 0;
        for (var index = 1; index <= tariff.RateCount; index++)
        {
            start += start == 0 ? tariff.RangeStart : tariff.RangeInterval;
            var end = start + tariff.RangeInterval - tariff.RangeStart;
            var rate = decimal.ToInt32((start - tariff.RangeStart) / tariff.RangeInterval) * tariff.RateInterval + tariff.RateStart;
            var adjustment = adjustments.FirstOrDefault(x => x.RangeStart == start && x.RangeEnd == end);
            var effectiveStart = adjustment is not null && adjustment.AdjustedEnd >= adjustment.AdjustedStart && adjustment.AdjustedEnd > 0 ? adjustment.AdjustedStart : start;
            var effectiveEnd = adjustment is not null && adjustment.AdjustedEnd >= adjustment.AdjustedStart && adjustment.AdjustedEnd > 0 ? adjustment.AdjustedEnd : end;
            if (adjustment is not null && adjustment.Rate > 0) rate = adjustment.Rate;
            if (tariff.RoundToFiveCents)
            {
                var missing = (((rate - decimal.Truncate(rate)) * 100) % 5) / 100;
                if (missing > 0) rate += 0.05m - missing;
            }
            var minimum = Math.Max(0, decimal.ToInt32(decimal.Ceiling(effectiveStart * 1000)) - 1);
            var maximum = decimal.ToInt32(decimal.Floor(effectiveEnd * 1000));
            if (maximum > minimum && !result.Any(x => x.MinimumGrams == minimum && x.MaximumGrams == maximum)) result.Add(new ExpandedBand(minimum, maximum, decimal.Round(rate, 2)));
        }
        return result;
    }

    private static async Task<List<LegacyService>> ReadServicesAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, """
            SELECT DISTINCT ST.servicioTipoID, SC.nombreCategoria, ST.nombreTipo, T.viaEncaminamientoID,
                   CASE T.viaEncaminamientoID
                     WHEN 1 THEN 'Aéreo' WHEN 2 THEN 'APR/SAL' WHEN 3 THEN 'Superficie'
                     WHEN 4 THEN 'Teléfono' WHEN 5 THEN 'Radio' WHEN 6 THEN 'Fax'
                     WHEN 7 THEN 'Medio Electrónico' WHEN 8 THEN 'Postal' WHEN 9 THEN 'Telegráfica'
                     WHEN 10 THEN 'Teléfono a Teléfono' WHEN 11 THEN 'Radio a Teléfono'
                     WHEN 12 THEN 'Teléfono a Radio' WHEN 13 THEN 'Radio a Radio'
                     ELSE CONCAT('Vía ', T.viaEncaminamientoID) END
            FROM [Servicios.Tipos] ST
            INNER JOIN [Servicios.Categorias] SC ON SC.servicioCategoriaID = ST.servicioCategoriaID
            INNER JOIN Servicios S ON S.servicioID = SC.servicioID
            INNER JOIN Tarifas T ON T.servicioTipoID = ST.servicioTipoID AND T.activo = 1
            WHERE S.activo = 1 AND SC.activo = 1 AND ST.activo = 1
              AND SC.ventaTipoID IN ('ENVIO_POSTAL','EXPRESO_POSTAL','EXPORTA_FACIL')
            ORDER BY SC.nombreCategoria, ST.nombreTipo, T.viaEncaminamientoID
            """, r => new LegacyService(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetString(4)), token);

    private static async Task<List<LegacyCountry>> ReadCountriesAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT paisID, paisNombre, ISNULL(numeroISO2,'') FROM Paises ORDER BY paisID", r => new LegacyCountry(r.GetInt32(0), r.GetString(1), r.GetString(2).Trim().ToUpperInvariant()), token);

    private static async Task<List<LegacyProvince>> ReadProvincesAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT provinciaPanamaID, nombreProvincia FROM ProvinciasPanama ORDER BY provinciaPanamaID", r => new LegacyProvince(r.GetInt32(0), r.GetString(1)), token);

    private static async Task<List<LegacyPair>> ReadPairsAsync(SqlConnection connection, string sql, CancellationToken token) =>
        await ReadAsync(connection, sql, r => new LegacyPair(r.GetInt32(0), r.GetInt32(1)), token);

    private static async Task<List<LegacyTariff>> ReadTariffsAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, """
            SELECT tarifaID, servicioTipoID, ISNULL(grupoID,0), ISNULL(paisID,0), ISNULL(viaEncaminamientoID,0),
                   rangoInicio, rangoFin, rangoIntervalo, cantidadTasas, tasaInicio, tasaIntervalo, tarifaAmbitoID, aplicarRedondeo
            FROM Tarifas WHERE activo = 1
            """, r => new LegacyTariff(r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetInt32(4), r.GetDecimal(5), r.GetDecimal(6), r.GetDecimal(7), r.GetInt32(8), r.GetDecimal(9), r.GetDecimal(10), r.GetInt32(11), r.GetBoolean(12)), token);

    private static async Task<List<LegacyAdjustment>> ReadAdjustmentsAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT tarifaID, rangoInicio, rangoFin, rangoInicioAjuste, rangoFinAjuste, tasaAjuste FROM [Tarifas.Ajustes]", r => new LegacyAdjustment(r.GetInt32(0), r.GetDecimal(1), r.GetDecimal(2), r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5)), token);

    private static async Task<List<LegacySupplemental>> ReadSupplementalsAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT servicioSuplementarioID, nombreServicioSuplementario, precio FROM [Servicios.Suplementarios]", r => new LegacySupplemental(r.GetInt32(0), r.GetString(1), r.GetDecimal(2)), token);

    private static async Task<List<LegacySupplementalLink>> ReadSupplementalLinksAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT tarifaID, servicioSuplementarioID, precioAjuste FROM [Tarifas.ServiciosSuplementarios]", r => new LegacySupplementalLink(r.GetInt32(0), r.GetInt32(1), r.GetDecimal(2)), token);

    private static async Task<List<LegacyCodeType>> ReadCodeTypesAsync(SqlConnection connection, CancellationToken token) =>
        await ReadAsync(connection, "SELECT codigoEnvioTipoID, ISNULL(aplicaServicioSuplementarioID,''), ISNULL(aplicaServicioTipoID,'') FROM [CodigosEnvios.Tipos]", r => new LegacyCodeType(r.GetInt32(0), ParseIds(Convert.ToString(r.GetValue(1)) ?? ""), ParseIds(Convert.ToString(r.GetValue(2)) ?? "")), token);

    private static async Task<Dictionary<int, string>> ResolvePrefixesAsync(SqlConnection connection, IEnumerable<int> codeTypeIds, CancellationToken token)
    {
        var result = new Dictionary<int, string>();
        foreach (var id in codeTypeIds)
        {
            await using var command = new SqlCommand("dbo.CodigoEnvioFormatoVigentePorCategoriaAnio", connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddWithValue("@codigoEnvioTipo", id); command.Parameters.AddWithValue("@anio", DateTime.Now.Year);
            try
            {
                await using var reader = await command.ExecuteReaderAsync(token);
                if (await reader.ReadAsync(token))
                {
                    var prefix = (Convert.ToString(reader["letraPosicionUno"]) + Convert.ToString(reader["letraPosicionDos"])).Trim().ToUpperInvariant();
                    if (prefix.Length == 2) result[id] = prefix;
                }
            }
            catch (SqlException)
            {
                // Algunos tipos históricos no tienen formato vigente; no deben impedir importar el resto del catálogo.
            }
            if (!result.ContainsKey(id))
            {
                await using var fallback = new SqlCommand("SELECT TOP (1) letraPosicionUno, letraPosicionDos FROM [CodigosEnvios.TiposFormatoVigentes] WHERE codigoEnvioTipoID = @tipo AND anio = @anio", connection);
                fallback.Parameters.AddWithValue("@tipo", id); fallback.Parameters.AddWithValue("@anio", DateTime.Now.Year);
                await using var fallbackReader = await fallback.ExecuteReaderAsync(token);
                if (await fallbackReader.ReadAsync(token))
                {
                    var prefix = (Convert.ToString(fallbackReader[0]) + Convert.ToString(fallbackReader[1])).Trim().ToUpperInvariant();
                    if (prefix.Length == 2) result[id] = prefix;
                }
            }
        }
        return result;
    }

    private static HashSet<int> ParseIds(string value) => value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(item => int.TryParse(item, out var id) ? id : 0).Where(id => id > 0).ToHashSet();

    private static async Task<List<T>> ReadAsync<T>(SqlConnection connection, string sql, Func<SqlDataReader, T> map, CancellationToken token)
    {
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await using var reader = await command.ExecuteReaderAsync(token);
        var result = new List<T>(); while (await reader.ReadAsync(token)) result.Add(map(reader)); return result;
    }

    internal sealed record LegacyTariff(int Id, int ServiceTypeId, int GroupId, int CountryId, int RouteId, decimal RangeStart, decimal RangeEnd, decimal RangeInterval, int RateCount, decimal RateStart, decimal RateInterval, int ScopeId, bool RoundToFiveCents);
    internal sealed record LegacyAdjustment(int TariffId, decimal RangeStart, decimal RangeEnd, decimal AdjustedStart, decimal AdjustedEnd, decimal Rate);
    internal sealed record ExpandedBand(int MinimumGrams, int MaximumGrams, decimal Price);
    private sealed record LegacyService(int ServiceTypeId, string CategoryName, string TypeName, int RouteId, string RouteName);
    private sealed record LegacyCountry(int Id, string Name, string Iso2);
    private sealed record LegacyProvince(int Id, string Name);
    private sealed record LegacyPair(int Left, int Right);
    private sealed record LegacyDestination(int LegacyId, bool IsDomestic, string Name, string Zone);
    private sealed record LegacySupplemental(int Id, string Name, decimal Price);
    private sealed record LegacySupplementalLink(int TariffId, int SupplementaryId, decimal PriceAdjustment);
    private sealed record LegacyCodeType(int Id, HashSet<int> SupplementaryIds, HashSet<int> ServiceTypeIds);
}
