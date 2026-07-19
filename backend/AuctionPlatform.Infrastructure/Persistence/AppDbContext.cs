using AuctionPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Auction> Auctions => Set<Auction>();
    public DbSet<AuctionCategory> AuctionCategories => Set<AuctionCategory>();
    public DbSet<AuctionItem> AuctionItems => Set<AuctionItem>();
    public DbSet<AuctionItemImage> AuctionItemImages => Set<AuctionItemImage>();
    public DbSet<AuctionWinner> AuctionWinners => Set<AuctionWinner>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
