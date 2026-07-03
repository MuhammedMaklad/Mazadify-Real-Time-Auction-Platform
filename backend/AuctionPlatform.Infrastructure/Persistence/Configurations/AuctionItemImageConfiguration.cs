using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AuctionItemImageConfiguration : IEntityTypeConfiguration<AuctionItemImage>
{
    public void Configure(EntityTypeBuilder<AuctionItemImage> builder)
    {
        builder.ToTable("AuctionItemImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasIndex(e => e.AuctionItemId);
    }
}
