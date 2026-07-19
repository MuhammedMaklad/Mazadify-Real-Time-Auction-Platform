using AuctionPlatform.Application.AutoBids.DTOs;
using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.AutoBids.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuctionPlatform.WebApi.Controllers
{
    [Route("api/auctions/{auctionId:guid}/auto-bids")]
    [ApiController]
    [Authorize]
    public class AutoBidsController : ControllerBase
    {
        private readonly IAutoBidService _autoBidService;
        private readonly CreateAutoBidValidator _createValidator;
        private readonly UpdateAutoBidValidator _updateValidator;

        public AutoBidsController(
            IAutoBidService autoBidService,
            CreateAutoBidValidator createValidator,
            UpdateAutoBidValidator updateValidator)
        {
            _autoBidService = autoBidService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        private Guid? ResolveCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            [FromRoute] Guid auctionId,
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var result = await _autoBidService.GetByIdAsync(id, ct);

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyAutoBid(
            [FromRoute] Guid auctionId,
            CancellationToken ct)
        {
            var bidderId = ResolveCurrentUserId();
            if (bidderId is null)
                return Unauthorized(new { message = "User identity claim not found or invalid." });

            var result = await _autoBidService
                .GetUserAutoBidAsync(auctionId, bidderId.Value, ct);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromRoute] Guid auctionId,
            [FromBody] CreateAutoBidRequest request,
            CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);

            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var bidderId = ResolveCurrentUserId();
            if (bidderId is null)
                return Unauthorized(new { message = "User identity claim not found or invalid." });

            var result = await _autoBidService
                .CreateAsync(auctionId, bidderId.Value, request, ct);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    auctionId,
                    id = result.Id
                },
                result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid auctionId,
            [FromRoute] Guid id,
            [FromBody] UpdateAutoBidRequest request,
            CancellationToken ct)
        {
            var validation = await _updateValidator.ValidateAsync(request, ct);

            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var bidderId = ResolveCurrentUserId();
            if (bidderId is null)
                return Unauthorized(new { message = "User identity claim not found or invalid." });

            var result = await _autoBidService.UpdateAsync(
                id,
                bidderId.Value,
                request,
                ct);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid auctionId,
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var bidderId = ResolveCurrentUserId();
            if (bidderId is null)
                return Unauthorized(new { message = "User identity claim not found or invalid." });

            await _autoBidService.DeleteAsync(
                id,
                bidderId.Value,
                ct);

            return NoContent();
        }
    }
}
