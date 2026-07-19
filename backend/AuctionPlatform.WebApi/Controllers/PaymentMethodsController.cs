using System.Security.Claims;
using AuctionPlatform.Application.PaymentMethods.DTOs;
using AuctionPlatform.Application.PaymentMethods.Interfaces;
using AuctionPlatform.Application.PaymentMethods.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.WebApi.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly AddPaymentMethodRequestValidator _addValidator;

    public PaymentMethodsController(
        IPaymentMethodService paymentMethodService,
        AddPaymentMethodRequestValidator addValidator)
    {
        _paymentMethodService = paymentMethodService;
        _addValidator = addValidator;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User is not authenticated."));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _paymentMethodService.GetByUserAsync(CurrentUserId, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddPaymentMethodRequest request, CancellationToken ct)
    {
        var validation = await _addValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _paymentMethodService.AddAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPatch("{id}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        await _paymentMethodService.SetDefaultAsync(CurrentUserId, id, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _paymentMethodService.DeactivateAsync(CurrentUserId, id, ct);
        return NoContent();
    }
}
