using System.Reflection;
using AuctionPlatform.Application.Auth.Interfaces;
using AuctionPlatform.Application.Auth.Services;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.Services;
using AuctionPlatform.Application.Categories.Interfaces;
using AuctionPlatform.Application.Categories.Services;
using AuctionPlatform.Application.PaymentMethods.Interfaces;
using AuctionPlatform.Application.PaymentMethods.Services;
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
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();

        return services;
    }
}
