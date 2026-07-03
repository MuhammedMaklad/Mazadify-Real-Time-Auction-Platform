using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
{
    public void Configure(EntityTypeBuilder<Auction> builder)
    {
        builder.ToTable("Auctions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(e => e.StartingPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ReservePrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.CurrentHighestBid)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.BidIncrement)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.DeliveryType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.DeliveryNotes)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.SellerId);
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.StartTime, e.EndTime });

        builder.HasOne(e => e.Seller)
            .WithMany(e => e.CreatedAuctions)
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Auctions)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Items)
            .WithOne(e => e.Auction)
            .HasForeignKey(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Winner)
            .WithOne(e => e.Auction)
            .HasForeignKey<AuctionWinner>(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
