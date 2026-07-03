using AuctionPlatform.Application.Auctions.DTOs;
using AuctionPlatform.Application.Categories.DTOs;
using AuctionPlatform.Domain.Entities;
using AutoMapper;

namespace AuctionPlatform.Application.Mappings;

public class AuctionProfile : Profile
{
    public AuctionProfile()
    {
        CreateMap<Auction, AuctionDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DeliveryType, o => o.MapFrom(s => s.DeliveryType.ToString()))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
            .ForMember(d => d.Seller, o => o.MapFrom(s => s.Seller))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<Auction, AuctionSummaryDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DeliveryType, o => o.MapFrom(s => s.DeliveryType.ToString()))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SellerUsername, o => o.MapFrom(s => s.Seller.UserName))
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count))
            .ForMember(d => d.PrimaryImageUrl,
                o => o.MapFrom(s => s.Items
                    .SelectMany(i => i.Images)
                    .Where(img => img.IsPrimary)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault()));

        CreateMap<AuctionItem, AuctionItemDto>()
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images));

        CreateMap<AuctionItemImage, AuctionItemImageDto>();

        CreateMap<User, UserBriefDto>();

        CreateMap<AuctionCategory, AuctionCategoryDto>();
    }
}
