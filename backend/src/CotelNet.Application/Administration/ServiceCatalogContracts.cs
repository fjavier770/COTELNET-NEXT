namespace CotelNet.Application.Administration;

public sealed record PostalServiceAdminDto(int Id, string Code, string Name, string S10Prefix, bool Active);
public sealed record SavePostalServiceRequest(string Code, string Name, string S10Prefix, bool Active = true);
public sealed record DestinationAdminDto(int Id, string Code, string Name, string Zone, bool IsDomestic, bool Active);
public sealed record SaveDestinationRequest(string Code, string Name, string Zone, bool IsDomestic, bool Active = true);
public sealed record WeightTariffAdminDto(int Id, int PostalServiceId, string PostalService, string DestinationZone, int MinimumWeightGrams, int MaximumWeightGrams, decimal Price, bool Active);
public sealed record SaveWeightTariffRequest(int PostalServiceId, string DestinationZone, int MinimumWeightGrams, int MaximumWeightGrams, decimal Price, bool Active = true);
public sealed record SupplementaryServiceAdminDto(int Id, string Code, string Name, decimal Price, bool Active);
public sealed record SaveSupplementaryServiceRequest(string Code, string Name, decimal Price, bool Active = true);

public interface IServiceCatalogService
{
    Task<IReadOnlyList<PostalServiceAdminDto>> GetPostalServicesAsync(CancellationToken cancellationToken);
    Task<PostalServiceAdminDto> CreatePostalServiceAsync(SavePostalServiceRequest request, CancellationToken cancellationToken);
    Task<PostalServiceAdminDto?> UpdatePostalServiceAsync(int id, SavePostalServiceRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<DestinationAdminDto>> GetDestinationsAsync(CancellationToken cancellationToken);
    Task<DestinationAdminDto> CreateDestinationAsync(SaveDestinationRequest request, CancellationToken cancellationToken);
    Task<DestinationAdminDto?> UpdateDestinationAsync(int id, SaveDestinationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<WeightTariffAdminDto>> GetWeightTariffsAsync(CancellationToken cancellationToken);
    Task<WeightTariffAdminDto> CreateWeightTariffAsync(SaveWeightTariffRequest request, CancellationToken cancellationToken);
    Task<WeightTariffAdminDto?> UpdateWeightTariffAsync(int id, SaveWeightTariffRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplementaryServiceAdminDto>> GetSupplementaryServicesAsync(CancellationToken cancellationToken);
    Task<SupplementaryServiceAdminDto> CreateSupplementaryServiceAsync(SaveSupplementaryServiceRequest request, CancellationToken cancellationToken);
    Task<SupplementaryServiceAdminDto?> UpdateSupplementaryServiceAsync(int id, SaveSupplementaryServiceRequest request, CancellationToken cancellationToken);
}
