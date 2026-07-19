using AuctionPlatform.Application.PaymentMethods.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;

namespace AuctionPlatform.Application.Mappings;

public class PaymentMethodProfile : Profile
{
    public PaymentMethodProfile()
    {
        CreateMap<PaymentMethod, PaymentMethodDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
    }
}
