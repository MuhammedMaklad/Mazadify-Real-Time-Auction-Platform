using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AutoBidConfiguration : IEntityTypeConfiguration<AutoBid>
{
    public void Configure(EntityTypeBuilder<AutoBid> builder)
    {
        builder.ToTable("AutoBids");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MaxAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Auction)
            .WithMany(a => a.AutoBids)
            .HasForeignKey(x => x.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Bidder)
            .WithMany(u => u.AutoBids)
            .HasForeignKey(x => x.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.AuctionId,
            x.BidderId
        }).IsUnique();
    }
}

