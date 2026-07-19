using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Indexes on Email and UserName are created by IdentityDbContext.
        // We add explicit non-clustered indexes for query performance.
        builder.HasIndex(e => e.Email).HasDatabaseName("IX_Users_Email");
        builder.HasIndex(e => e.UserName).HasDatabaseName("IX_Users_UserName");

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
