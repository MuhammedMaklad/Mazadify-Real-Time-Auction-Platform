using System.Reflection;
using AuctionPlatform.Application.Auth.Interfaces;
using AuctionPlatform.Application.Auth.Services;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.Services;
using AuctionPlatform.Application.AutoBids.Interfaces;
using AuctionPlatform.Application.AutoBids.Services;
using AuctionPlatform.Application.Categories.Interfaces;
using AuctionPlatform.Application.Categories.Services;
using AuctionPlatform.Application.PaymentMethods.Interfaces;
using AuctionPlatform.Application.PaymentMethods.Services;
using AuctionPlatform.Application.Winners.Interfaces;
using AuctionPlatform.Application.Winners.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);

        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<IBidAuditService, BidAuditService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        
        services.AddScoped<IAutoBidService, AutoBidService>();

        services.AddScoped<IAutoBidEngine, AutoBidEngine>();

        services.AddScoped<IAuctionWinnerService, AuctionWinnerService>();

        return services;
    }
}
