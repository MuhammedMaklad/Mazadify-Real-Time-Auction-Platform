using AuctionPlatform.Application.Notifications.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;

namespace AuctionPlatform.Application.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<CreateNotificationRequest, Notification>();
    }
}
