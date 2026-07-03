using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(e => e.CreatedAuctions)
            .WithOne(e => e.Seller)
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WonAuction)
            .WithOne(e => e.Winner)
            .HasForeignKey<AuctionWinner>(e => e.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
