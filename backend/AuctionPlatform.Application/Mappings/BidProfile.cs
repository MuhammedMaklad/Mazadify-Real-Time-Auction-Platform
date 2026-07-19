using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;

namespace AuctionPlatform.Application.Mappings;

public class BidProfile : Profile
{
    public BidProfile()
    {
        CreateMap<Bid, BidDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Bidder, o => o.MapFrom(s => s.Bidder));
    }
}
