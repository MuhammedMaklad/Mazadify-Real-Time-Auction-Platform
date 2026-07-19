using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Provider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.LastFour)
            .HasMaxLength(4);

        builder.Property(e => e.ExpiryMonth)
            .HasMaxLength(2);

        builder.Property(e => e.ExpiryYear)
            .HasMaxLength(4);

        builder.HasOne(e => e.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
