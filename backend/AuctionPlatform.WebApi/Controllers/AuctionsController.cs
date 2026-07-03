using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.Validators;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.WebApi.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly CreateAuctionRequestValidator _createValidator;
    private readonly UpdateAuctionRequestValidator _updateValidator;
    private readonly AuctionListFilterValidator _filterValidator;

    public AuctionsController(
        IAuctionService auctionService,
        CreateAuctionRequestValidator createValidator,
        UpdateAuctionRequestValidator updateValidator,
        AuctionListFilterValidator filterValidator)
    {
        _auctionService = auctionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] AuctionListFilter filter, CancellationToken ct)
    {
        var validation = await _filterValidator.ValidateAsync(filter, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _auctionService.GetListAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _auctionService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuctionRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var sellerId = Guid.Parse("B1000000-0000-0000-0000-000000000002");
        var result = await _auctionService.CreateAsync(sellerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuctionRequest request, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _auctionService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _auctionService.DeleteAsync(id, ct);
        return NoContent();
    }
}
