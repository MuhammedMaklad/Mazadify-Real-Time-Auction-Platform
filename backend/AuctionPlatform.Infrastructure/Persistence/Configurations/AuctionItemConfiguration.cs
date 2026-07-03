using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AuctionItemConfiguration : IEntityTypeConfiguration<AuctionItem>
{
    public void Configure(EntityTypeBuilder<AuctionItem> builder)
    {
        builder.ToTable("AuctionItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Condition)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.AuctionId);

        builder.HasMany(e => e.Images)
            .WithOne(e => e.AuctionItem)
            .HasForeignKey(e => e.AuctionItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
