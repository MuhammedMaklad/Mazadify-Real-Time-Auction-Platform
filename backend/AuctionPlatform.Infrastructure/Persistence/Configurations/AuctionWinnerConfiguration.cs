using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AuctionWinnerConfiguration : IEntityTypeConfiguration<AuctionWinner>
{
    public void Configure(EntityTypeBuilder<AuctionWinner> builder)
    {
        builder.ToTable("AuctionWinners");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FinalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.ShippingCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.PaymentStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.DeliveryStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.TrackingNumber)
            .HasMaxLength(100);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.AuctionId).IsUnique();
        builder.HasIndex(e => e.WinnerId);

        builder.HasOne(e => e.Auction)
            .WithOne(a => a.Winner)
            .HasForeignKey<AuctionWinner>(e => e.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Winner)
            .WithOne(e => e.WonAuction)
            .HasForeignKey<AuctionWinner>(e => e.WinnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WinningBid)
            .WithOne(e => e.AuctionWinner)
            .HasForeignKey<AuctionWinner>(e => e.WinningBidId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PaymentMethod)
            .WithMany(e => e.SettledPayments)
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
