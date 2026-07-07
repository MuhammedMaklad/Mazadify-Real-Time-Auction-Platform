using AuctionPlatform.Application.AutoBids.DTOs;
using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.AutoBids.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.WebApi.Controllers
{
    [Route("api/auctions/{auctionId:guid}/auto-bids")]
    [ApiController]
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
            // TODO: Extract bidderId from authenticated user (JWT claims) when Member 1 finishes Auth.
            var bidderId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

            var result = await _autoBidService
                .GetUserAutoBidAsync(auctionId, bidderId, ct);

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

            // TODO: Extract bidderId from authenticated user (JWT claims) when Member 1 finishes Auth.
            var bidderId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

            var result = await _autoBidService
                .CreateAsync(auctionId, bidderId, request, ct);

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

            // TODO: Extract bidderId from authenticated user (JWT claims) when Member 1 finishes Auth.
            var bidderId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

            var result = await _autoBidService.UpdateAsync(
                id,
                bidderId,
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
            // TODO: Extract bidderId from authenticated user (JWT claims) when Member 1 finishes Auth.
            var bidderId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

            await _autoBidService.DeleteAsync(
                id,
                bidderId,
                ct);

            return NoContent();
        }
    }
}
