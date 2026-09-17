using System.Security.Claims;
using CotelNet.Application.Administration;
using CotelNet.Application.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotelNet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class SalesController(ISalesService service) : ControllerBase
{
    [HttpGet("sales/catalog")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public async Task<IActionResult> Catalog(CancellationToken cancellationToken) => Ok(await service.GetCatalogAsync(UserId(), cancellationToken));

    [HttpGet("cash/catalog")]
    [Authorize(Policy = PermissionCodes.CajaAccess)]
    public async Task<IActionResult> CashCatalog(CancellationToken cancellationToken) => Ok(await service.GetCatalogAsync(UserId(), cancellationToken));

    [HttpGet("sales/recent")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public async Task<IActionResult> Recent(CancellationToken cancellationToken) => Ok(await service.GetRecentSalesAsync(UserId(), cancellationToken));

    [HttpGet("sales/senders/{document}")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public async Task<IActionResult> Sender(string document, CancellationToken cancellationToken) => Ok(await service.GetSenderProfileAsync(document, cancellationToken));

    [HttpGet("sales/cash/current")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public async Task<IActionResult> SalesCurrentCash(CancellationToken cancellationToken) => Ok(await service.GetCurrentCashAsync(UserId(), cancellationToken));

    [HttpPost("sales")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public Task<IActionResult> CreateSale(CreateSaleRequest request, CancellationToken cancellationToken) => ExecuteAsync(() => service.CreateSaleAsync(UserId(), request, cancellationToken));

    [HttpPost("sales/quote")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public Task<IActionResult> QuoteShipment(QuoteShipmentRequest request, CancellationToken cancellationToken) => ExecuteAsync(() => service.QuoteShipmentAsync(request, cancellationToken));

    [HttpPost("sales/shipments")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public Task<IActionResult> CreateShipment(CreateShipmentRequest request, CancellationToken cancellationToken) => ExecuteAsync(() => service.CreateShipmentAsync(UserId(), request, cancellationToken));

    [HttpGet("sales/{id:int}")]
    [Authorize(Policy = PermissionCodes.VentasAccess)]
    public async Task<IActionResult> GetSale(int id, CancellationToken cancellationToken)
    {
        var sale = await service.GetSaleAsync(UserId(), id, cancellationToken);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpGet("cash/current")]
    [Authorize(Policy = PermissionCodes.CajaAccess)]
    public async Task<IActionResult> CurrentCash(CancellationToken cancellationToken) => Ok(await service.GetCurrentCashAsync(UserId(), cancellationToken));

    [HttpPost("cash/open")]
    [Authorize(Policy = PermissionCodes.CajaAccess)]
    public Task<IActionResult> OpenCash(OpenCashRequest request, CancellationToken cancellationToken) => ExecuteAsync(() => service.OpenCashAsync(UserId(), request, cancellationToken));

    [HttpPost("cash/close")]
    [Authorize(Policy = PermissionCodes.CajaAccess)]
    public Task<IActionResult> CloseCash(CloseCashRequest request, CancellationToken cancellationToken) => ExecuteAsync(() => service.CloseCashAsync(UserId(), request, cancellationToken));

    private int UserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException("El token no identifica al usuario.");
    }

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
