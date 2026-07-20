using AuctionPlatform.Application.Winners.DTOs;
using AuctionPlatform.Application.Winners.Interfaces;
using AuctionPlatform.Application.Winners.Validators;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuctionPlatform.WebApi.Controllers
{
    [Route("api/auctions/{id:guid}")]
    [ApiController]
    public class WinnersController : ControllerBase
    {
        private readonly IAuctionWinnerService _winnerService;
        private readonly UpdateDeliveryStatusValidator _deliveryStatusValidator;
        private readonly UpdateTrackingNumberValidator _trackingNumberValidator;

        public WinnersController(
            IAuctionWinnerService winnerService,
            UpdateDeliveryStatusValidator deliveryStatusValidator,
            UpdateTrackingNumberValidator trackingNumberValidator)
        {
            _winnerService = winnerService;
            _deliveryStatusValidator = deliveryStatusValidator;
            _trackingNumberValidator = trackingNumberValidator;
        }

        [HttpGet("winner")]
        public async Task<IActionResult> GetWinner(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var result = await _winnerService.GetWinnerByAuctionIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost("settle")]
        public async Task<IActionResult> Settle(
            [FromRoute] Guid id,
            CancellationToken ct)
        {
            var result = await _winnerService.SettleAuctionAsync(id, ct);
            if (result is null)
                return Ok("Auction ended with no winner (reserve price not met or no bids).");

            return Ok(result);
        }

        [HttpPut("winner/delivery")]
        public async Task<IActionResult> UpdateDelivery(
            [FromRoute] Guid id,
            [FromBody] UpdateDeliveryStatusRequest request,
            CancellationToken ct)
        {
            var validation = await _deliveryStatusValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var result = await _winnerService.UpdateDeliveryStatusAsync(id, request, ct);
            return Ok(result);
        }

        [HttpPut("winner/tracking")]
        public async Task<IActionResult> UpdateTracking(
            [FromRoute] Guid id,
            [FromBody] UpdateTrackingNumberRequest request,
            CancellationToken ct)
        {
            var validation = await _trackingNumberValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(validation.Errors);

            var result = await _winnerService.UpdateTrackingNumberAsync(id, request, ct);
            return Ok(result);
        }
    }
}
