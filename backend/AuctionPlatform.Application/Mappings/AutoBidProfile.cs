using AuctionPlatform.Application.AutoBids.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuctionPlatform.Application.Mappings
{
    public class AutoBidProfile : Profile
    {
        public AutoBidProfile()
        {
            CreateMap<AutoBid, AutoBidDto>();
        }
    }
}
