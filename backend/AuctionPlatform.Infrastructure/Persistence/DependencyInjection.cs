using AuctionPlatform.Application;
using AuctionPlatform.Application.Auctions.Services;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Infrastructure.Caching;
using AuctionPlatform.Infrastructure.Notifications;
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
        services.AddScoped<IBidRepository, BidRepository>();

        // Temporary no-op notification service. Will be replaced by Real-Time module.
        services.AddScoped<INotificationService, NoOpNotificationService>();

        // Idempotency cache: uses distributed cache (Redis or in-memory)
        services.AddMemoryCache();
        services.AddDistributedMemoryCache(); // In-memory for development (Redis in production)
        services.AddScoped<IIdempotencyCache, DistributedIdempotencyCache>();

        services.AddScoped<IAutoBidRepository, AutoBidRepository>();

        services.AddApplication();

        return services;
    }
}
