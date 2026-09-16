using CotelNet.Application.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotelNet.Api.Controllers;

[ApiController]
[Authorize(Policy = PermissionCodes.ServicesManage)]
[Route("api/v1/administration/service-catalog")]
public sealed class ServiceCatalogController(IServiceCatalogService service) : ControllerBase
{
    [HttpGet("postal-services")]
    public async Task<IActionResult> GetPostalServices(CancellationToken token) => Ok(await service.GetPostalServicesAsync(token));
    [HttpPost("postal-services")]
    public Task<IActionResult> CreatePostalService(SavePostalServiceRequest request, CancellationToken token) => ExecuteAsync(() => service.CreatePostalServiceAsync(request, token));
    [HttpPut("postal-services/{id:int}")]
    public Task<IActionResult> UpdatePostalService(int id, SavePostalServiceRequest request, CancellationToken token) => ExecuteNullableAsync(() => service.UpdatePostalServiceAsync(id, request, token));

    [HttpGet("destinations")]
    public async Task<IActionResult> GetDestinations(CancellationToken token) => Ok(await service.GetDestinationsAsync(token));
    [HttpPost("destinations")]
    public Task<IActionResult> CreateDestination(SaveDestinationRequest request, CancellationToken token) => ExecuteAsync(() => service.CreateDestinationAsync(request, token));
    [HttpPut("destinations/{id:int}")]
    public Task<IActionResult> UpdateDestination(int id, SaveDestinationRequest request, CancellationToken token) => ExecuteNullableAsync(() => service.UpdateDestinationAsync(id, request, token));

    [HttpGet("weight-tariffs")]
    public async Task<IActionResult> GetWeightTariffs(CancellationToken token) => Ok(await service.GetWeightTariffsAsync(token));
    [HttpPost("weight-tariffs")]
    public Task<IActionResult> CreateWeightTariff(SaveWeightTariffRequest request, CancellationToken token) => ExecuteAsync(() => service.CreateWeightTariffAsync(request, token));
    [HttpPut("weight-tariffs/{id:int}")]
    public Task<IActionResult> UpdateWeightTariff(int id, SaveWeightTariffRequest request, CancellationToken token) => ExecuteNullableAsync(() => service.UpdateWeightTariffAsync(id, request, token));

    [HttpGet("supplementary-services")]
    public async Task<IActionResult> GetSupplementaryServices(CancellationToken token) => Ok(await service.GetSupplementaryServicesAsync(token));
    [HttpPost("supplementary-services")]
    public Task<IActionResult> CreateSupplementaryService(SaveSupplementaryServiceRequest request, CancellationToken token) => ExecuteAsync(() => service.CreateSupplementaryServiceAsync(request, token));
    [HttpPut("supplementary-services/{id:int}")]
    public Task<IActionResult> UpdateSupplementaryService(int id, SaveSupplementaryServiceRequest request, CancellationToken token) => ExecuteNullableAsync(() => service.UpdateSupplementaryServiceAsync(id, request, token));

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
    }

    private async Task<IActionResult> ExecuteNullableAsync<T>(Func<Task<T?>> action) where T : class
    {
        try { var result = await action(); return result is null ? NotFound() : Ok(result); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
    }
}
