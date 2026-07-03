using System.Reflection;
using AuctionPlatform.Application.Auctions.Interfaces;
using AuctionPlatform.Application.Auctions.Services;
using AuctionPlatform.Application.Categories.Interfaces;
using AuctionPlatform.Application.Categories.Services;
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

        return services;
    }
}
