using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.ToTable("Bids");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.PlacedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.IsAutoBid)
            .IsRequired()
            .HasDefaultValue(false);

        // Optimistic concurrency control
        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        // Indexes for query performance
        builder.HasIndex(e => e.AuctionId)
            .HasDatabaseName("IX_Bids_AuctionId");

        builder.HasIndex(e => e.BidderId)
            .HasDatabaseName("IX_Bids_BidderId");

        builder.HasIndex(e => e.PlacedAt)
            .HasDatabaseName("IX_Bids_PlacedAt");

        // Composite index for highest bid lookup
        builder.HasIndex(e => new { e.AuctionId, e.Amount })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Bids_AuctionId_Amount_Desc");

        // Foreign key relationships
        builder.HasOne(e => e.Auction)
            .WithMany(e => e.Bids)
            .HasForeignKey(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Bidder)
            .WithMany(e => e.Bids)
            .HasForeignKey(e => e.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AuctionWinner)
            .WithOne(e => e.WinningBid)
            .HasForeignKey<AuctionWinner>(e => e.WinningBidId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
