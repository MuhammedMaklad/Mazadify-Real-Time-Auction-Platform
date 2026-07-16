using AuctionPlatform.Application;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Hubs;
using AuctionPlatform.Infrastructure.Notifications.Services;

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
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? configuration.GetValue<string>("Redis:Configuration");

        services.AddSignalR()
            .AddStackExchangeRedis(redisConnection ?? "localhost:6379");




        services.AddApplication();

        return services;
    }
}
