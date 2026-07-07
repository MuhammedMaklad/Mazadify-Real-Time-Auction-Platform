using AuctionPlatform.Application.Winners.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;

namespace AuctionPlatform.Application.Mappings
{
    public class AuctionWinnerProfile : Profile
    {
        public AuctionWinnerProfile()
        {
            CreateMap<AuctionWinner, AuctionWinnerDto>();
        }
    }
}
