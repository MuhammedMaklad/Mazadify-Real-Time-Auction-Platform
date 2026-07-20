using AuctionPlatform.Application;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.Hubs;
using AuctionPlatform.Infrastructure.Notifications.Services;

using AuctionPlatform.Infrastructure.Persistence.Repositories;
using AuctionPlatform.Infrastructure.Services;
using AuctionPlatform.Infrastructure.Settings;
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

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddTransient<DbSeeder>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IAuctionCategoryRepository, AuctionCategoryRepository>();
        services.AddScoped<IAuctionItemRepository, AuctionItemRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        var redisConnection = configuration.GetConnectionString("Redis")
            ?? configuration.GetValue<string>("Redis:Configuration");

        var signalRBuilder = services.AddSignalR();
        if (!string.IsNullOrEmpty(redisConnection))
        {
            signalRBuilder.AddStackExchangeRedis(redisConnection);
        }




        services.AddApplication();

        return services;
    }
}
