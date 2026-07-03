using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AutoBidConfiguration : IEntityTypeConfiguration<AutoBid>
{
    public void Configure(EntityTypeBuilder<AutoBid> builder)
    {
        builder.ToTable("AutoBids");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MaxAmount)
            .HasColumnType("decimal(18,2)");
    }
}
