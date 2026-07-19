using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Common.Models;
using AuctionPlatform.Application.Hubs;
using AuctionPlatform.Application.Notifications.DTOs;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace AuctionPlatform.Infrastructure.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly IMapper _mapper;

    public NotificationService(
        INotificationRepository repository,
        IHubContext<AuctionHub> hubContext,
        IMapper mapper)
    {
        _repository = repository;
        _hubContext = hubContext;
        _mapper = mapper;
    }

    public async Task<NotificationDto> CreateAndBroadcastAsync(
        CreateNotificationRequest request,
        CancellationToken ct = default)
    {
        var notification = _mapper.Map<Notification>(request);
        notification.Id = Guid.NewGuid();
        notification.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(notification, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = _mapper.Map<NotificationDto>(notification);

        await _hubContext.Clients.Group($"user-{request.UserId}")
            .SendAsync("NotificationReceived", dto, ct);

        return dto;
    }

    public Task BroadcastAsync(
        string eventName,
        object payload,
        BroadcastTarget target,
        CancellationToken ct = default)
    {
        var clients = target switch
        {
            BroadcastTarget.All => _hubContext.Clients.All,
            BroadcastTarget.AuctionGroup g => _hubContext.Clients.Group($"auction-{g.AuctionId}"),
            BroadcastTarget.User u => _hubContext.Clients.Group($"user-{u.UserId}"),
            _ => _hubContext.Clients.All
        };

        return clients.SendAsync(eventName, payload, ct);
    }

    public async Task<PagedResult<NotificationDto>> GetUserNotificationsAsync(
        Guid userId,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var paged = await _repository.GetByUserAsync(userId, unreadOnly, page, pageSize, ct);

        return new PagedResult<NotificationDto>
        {
            Items = _mapper.Map<List<NotificationDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public Task MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken ct = default)
    {
        return _repository.MarkAsReadAsync(notificationId, userId, ct);
    }

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        return _repository.MarkAllAsReadAsync(userId, ct);
    }

    public async Task NotifyBidPlacedAsync(
        Guid auctionId,
        Guid bidderId,
        decimal amount,
        CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"auction-{auctionId}")
            .SendAsync("BidPlaced", new { auctionId, bidderId, amount }, cancellationToken: ct);
    }

    public async Task NotifyOutbidAsync(
        Guid auctionId,
        Guid previousBidderId,
        decimal currentHighestBid,
        CancellationToken ct = default)
    {
        var request = new CreateNotificationRequest
        {
            UserId = previousBidderId,
            Type = NotificationType.OutbidAlert,
            Title = "You have been outbid!",
            Message = $"Someone placed a higher bid on auction {auctionId}. Current highest bid is {currentHighestBid:C}.",
            RelatedAuctionId = auctionId,
            Payload = $"{{\"currentHighestBid\": {currentHighestBid}}}"
        };
        await CreateAndBroadcastAsync(request, ct);
    }

    public async Task NotifyYouWonAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default)
    {
        var request = new CreateNotificationRequest
        {
            UserId = winnerId,
            Type = NotificationType.YouWon,
            Title = "Congratulations! You won the auction!",
            Message = $"You won the auction {auctionId} with a bid of {finalPrice:C}.",
            RelatedAuctionId = auctionId,
            Payload = $"{{\"finalPrice\": {finalPrice}}}"
        };
        await CreateAndBroadcastAsync(request, ct);
    }

    public async Task NotifyPaymentRequiredAsync(
        Guid auctionId,
        Guid winnerId,
        decimal finalPrice,
        CancellationToken ct = default)
    {
        var request = new CreateNotificationRequest
        {
            UserId = winnerId,
            Type = NotificationType.PaymentRequired,
            Title = "Payment Required",
            Message = $"Payment is required for the won auction {auctionId}. Final price is {finalPrice:C}.",
            RelatedAuctionId = auctionId,
            Payload = $"{{\"finalPrice\": {finalPrice}}}"
        };
        await CreateAndBroadcastAsync(request, ct);
    }
}
