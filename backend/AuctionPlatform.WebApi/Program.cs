using System.Text;
using AuctionPlatform.Application.Hubs;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Infrastructure.Persistence;
using AuctionPlatform.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "Mazadify",
            ValidAudience = jwtSettings["Audience"] ?? "Mazadify.Users",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "MazadifySuperSecretKeyForDevOnly!123")),
            RoleClaimType = "role",
            NameClaimType = "sub"
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure rate limiting: per-user and per-IP policies
builder.Services.AddRateLimiter(options =>
{
    // Per-user rate limiter: 10 bids per minute per authenticated user
    options.AddSlidingWindowLimiter("per_user_bid_limit", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.SegmentsPerWindow = 2;
        config.PermitLimit = 10;
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 2;
    });

    // Per-IP rate limiter: 50 requests per minute (general API protection)
    options.AddSlidingWindowLimiter("per_ip_limit", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.SegmentsPerWindow = 2;
        config.PermitLimit = 50;
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 5;
    });

    // Default rate limiter rejection response
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Rate limit exceeded. Please try again later." },
            cancellationToken: token);
    };
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.MapGet("/", () => "AuctionPlatform.WebApi is running!");
app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
