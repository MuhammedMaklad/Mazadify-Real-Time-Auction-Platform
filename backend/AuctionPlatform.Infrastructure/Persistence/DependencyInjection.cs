using AuctionPlatform.Application;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionPlatform.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<DbSeeder>();

        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IAuctionCategoryRepository, AuctionCategoryRepository>();
        services.AddScoped<IAuctionItemRepository, AuctionItemRepository>();

        services.AddApplication();

        return services;
    }
}
