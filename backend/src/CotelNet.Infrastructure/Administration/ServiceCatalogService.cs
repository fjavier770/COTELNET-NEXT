using CotelNet.Application.Administration;
using CotelNet.Domain.Sales;
using CotelNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CotelNet.Infrastructure.Administration;

public sealed class ServiceCatalogService(CotelNetDbContext db) : IServiceCatalogService
{
    public async Task<IReadOnlyList<PostalServiceAdminDto>> GetPostalServicesAsync(CancellationToken cancellationToken) =>
        await db.PostalServices.AsNoTracking().OrderBy(x => x.Name).Select(x => new PostalServiceAdminDto(x.Id, x.Code, x.Name, x.Active)).ToListAsync(cancellationToken);

    public async Task<PostalServiceAdminDto> CreatePostalServiceAsync(SavePostalServiceRequest request, CancellationToken cancellationToken)
    {
        ValidateText(request.Code, request.Name); await EnsureServiceCodeAsync(null, request.Code, cancellationToken);
        var entity = new PostalService(request.Code, request.Name); if (!request.Active) entity.Update(request.Code, request.Name, false);
        db.PostalServices.Add(entity); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    public async Task<PostalServiceAdminDto?> UpdatePostalServiceAsync(int id, SavePostalServiceRequest request, CancellationToken cancellationToken)
    {
        ValidateText(request.Code, request.Name); var entity = await db.PostalServices.FindAsync([id], cancellationToken); if (entity is null) return null;
        await EnsureServiceCodeAsync(id, request.Code, cancellationToken); entity.Update(request.Code, request.Name, request.Active); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    public async Task<IReadOnlyList<DestinationAdminDto>> GetDestinationsAsync(CancellationToken cancellationToken) =>
        await db.Destinations.AsNoTracking().OrderByDescending(x => x.IsDomestic).ThenBy(x => x.Name).Select(x => new DestinationAdminDto(x.Id, x.Code, x.Name, x.Zone, x.IsDomestic, x.Active)).ToListAsync(cancellationToken);

    public async Task<DestinationAdminDto> CreateDestinationAsync(SaveDestinationRequest request, CancellationToken cancellationToken)
    {
        ValidateDestination(request); await EnsureDestinationCodeAsync(null, request.Code, cancellationToken);
        var entity = new Destination(request.Code, request.Name, request.Zone, request.IsDomestic); if (!request.Active) entity.Update(request.Code, request.Name, request.Zone, request.IsDomestic, false);
        db.Destinations.Add(entity); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    public async Task<DestinationAdminDto?> UpdateDestinationAsync(int id, SaveDestinationRequest request, CancellationToken cancellationToken)
    {
        ValidateDestination(request); var entity = await db.Destinations.FindAsync([id], cancellationToken); if (entity is null) return null;
        await EnsureDestinationCodeAsync(id, request.Code, cancellationToken); entity.Update(request.Code, request.Name, request.Zone, request.IsDomestic, request.Active); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    public async Task<IReadOnlyList<WeightTariffAdminDto>> GetWeightTariffsAsync(CancellationToken cancellationToken) =>
        await db.WeightTariffs.Include(x => x.PostalService).AsNoTracking().OrderBy(x => x.PostalService.Name).ThenBy(x => x.DestinationZone).ThenBy(x => x.MinimumWeightGrams)
            .Select(x => new WeightTariffAdminDto(x.Id, x.PostalServiceId, x.PostalService.Name, x.DestinationZone, x.MinimumWeightGrams, x.MaximumWeightGrams, x.Price, x.Active)).ToListAsync(cancellationToken);

    public async Task<WeightTariffAdminDto> CreateWeightTariffAsync(SaveWeightTariffRequest request, CancellationToken cancellationToken)
    {
        var service = await ValidateTariffAsync(null, request, cancellationToken);
        var entity = new WeightTariff(service.Id, request.DestinationZone, request.MinimumWeightGrams, request.MaximumWeightGrams, request.Price); if (!request.Active) entity.Update(service.Id, request.DestinationZone, request.MinimumWeightGrams, request.MaximumWeightGrams, request.Price, false);
        db.WeightTariffs.Add(entity); await db.SaveChangesAsync(cancellationToken); return ToDto(entity, service.Name);
    }

    public async Task<WeightTariffAdminDto?> UpdateWeightTariffAsync(int id, SaveWeightTariffRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.WeightTariffs.FindAsync([id], cancellationToken); if (entity is null) return null;
        var service = await ValidateTariffAsync(id, request, cancellationToken); entity.Update(service.Id, request.DestinationZone, request.MinimumWeightGrams, request.MaximumWeightGrams, request.Price, request.Active); await db.SaveChangesAsync(cancellationToken); return ToDto(entity, service.Name);
    }

    public async Task<IReadOnlyList<SupplementaryServiceAdminDto>> GetSupplementaryServicesAsync(CancellationToken cancellationToken) =>
        await db.SupplementaryServices.AsNoTracking().OrderBy(x => x.Name).Select(x => new SupplementaryServiceAdminDto(x.Id, x.Code, x.Name, x.Price, x.Active)).ToListAsync(cancellationToken);

    public async Task<SupplementaryServiceAdminDto> CreateSupplementaryServiceAsync(SaveSupplementaryServiceRequest request, CancellationToken cancellationToken)
    {
        ValidateText(request.Code, request.Name); ValidatePrice(request.Price); await EnsureSupplementaryCodeAsync(null, request.Code, cancellationToken);
        var entity = new SupplementaryService(request.Code, request.Name, request.Price); if (!request.Active) entity.Update(request.Code, request.Name, request.Price, false);
        db.SupplementaryServices.Add(entity); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    public async Task<SupplementaryServiceAdminDto?> UpdateSupplementaryServiceAsync(int id, SaveSupplementaryServiceRequest request, CancellationToken cancellationToken)
    {
        ValidateText(request.Code, request.Name); ValidatePrice(request.Price); var entity = await db.SupplementaryServices.FindAsync([id], cancellationToken); if (entity is null) return null;
        await EnsureSupplementaryCodeAsync(id, request.Code, cancellationToken); entity.Update(request.Code, request.Name, request.Price, request.Active); await db.SaveChangesAsync(cancellationToken); return ToDto(entity);
    }

    private async Task<PostalService> ValidateTariffAsync(int? id, SaveWeightTariffRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DestinationZone)) throw new InvalidOperationException("La zona de destino es obligatoria.");
        if (request.MinimumWeightGrams < 0 || request.MaximumWeightGrams <= request.MinimumWeightGrams) throw new InvalidOperationException("El rango de peso no es válido.");
        ValidatePrice(request.Price);
        var service = await db.PostalServices.SingleOrDefaultAsync(x => x.Id == request.PostalServiceId, cancellationToken) ?? throw new InvalidOperationException("El servicio postal no existe.");
        var zone = request.DestinationZone.Trim().ToUpper();
        if (await db.WeightTariffs.AnyAsync(x => x.Id != id && x.PostalServiceId == request.PostalServiceId && x.DestinationZone == zone && request.MinimumWeightGrams < x.MaximumWeightGrams && request.MaximumWeightGrams > x.MinimumWeightGrams, cancellationToken))
            throw new InvalidOperationException("El rango de peso se cruza con otra tarifa del mismo servicio y zona.");
        return service;
    }

    private async Task EnsureServiceCodeAsync(int? id, string code, CancellationToken token) { var normalized = code.Trim().ToUpper(); if (await db.PostalServices.AnyAsync(x => x.Id != id && x.Code == normalized, token)) throw new InvalidOperationException("El código del servicio ya existe."); }
    private async Task EnsureDestinationCodeAsync(int? id, string code, CancellationToken token) { var normalized = code.Trim().ToUpper(); if (await db.Destinations.AnyAsync(x => x.Id != id && x.Code == normalized, token)) throw new InvalidOperationException("El código del destino ya existe."); }
    private async Task EnsureSupplementaryCodeAsync(int? id, string code, CancellationToken token) { var normalized = code.Trim().ToUpper(); if (await db.SupplementaryServices.AnyAsync(x => x.Id != id && x.Code == normalized, token)) throw new InvalidOperationException("El código del servicio suplementario ya existe."); }
    private static void ValidateText(string code, string name) { if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("El código y el nombre son obligatorios."); }
    private static void ValidateDestination(SaveDestinationRequest request) { ValidateText(request.Code, request.Name); if (string.IsNullOrWhiteSpace(request.Zone)) throw new InvalidOperationException("La zona tarifaria es obligatoria."); }
    private static void ValidatePrice(decimal price) { if (price < 0) throw new InvalidOperationException("El precio no puede ser negativo."); }
    private static PostalServiceAdminDto ToDto(PostalService x) => new(x.Id, x.Code, x.Name, x.Active);
    private static DestinationAdminDto ToDto(Destination x) => new(x.Id, x.Code, x.Name, x.Zone, x.IsDomestic, x.Active);
    private static WeightTariffAdminDto ToDto(WeightTariff x, string service) => new(x.Id, x.PostalServiceId, service, x.DestinationZone, x.MinimumWeightGrams, x.MaximumWeightGrams, x.Price, x.Active);
    private static SupplementaryServiceAdminDto ToDto(SupplementaryService x) => new(x.Id, x.Code, x.Name, x.Price, x.Active);
}
