using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuctionPlatform.Infrastructure.Persistence.Configurations;

public class AuctionCategoryConfiguration : IEntityTypeConfiguration<AuctionCategory>
{
    public void Configure(EntityTypeBuilder<AuctionCategory> builder)
    {
        builder.ToTable("AuctionCategories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasIndex(e => e.ParentCategoryId);

        builder.HasOne(e => e.ParentCategory)
            .WithMany(e => e.SubCategories)
            .HasForeignKey(e => e.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Auctions)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        Seed(builder);
    }

    private static void Seed(EntityTypeBuilder<AuctionCategory> builder)
    {
        var electronics = Guid.Parse("A1000000-0000-0000-0000-000000000001");
        var collectiblesArt = Guid.Parse("A1000000-0000-0000-0000-000000000002");
        var vehicles = Guid.Parse("A1000000-0000-0000-0000-000000000003");
        var homeGarden = Guid.Parse("A1000000-0000-0000-0000-000000000004");

        var computers = Guid.Parse("A1000000-0000-0000-0000-000000000011");
        var smartphonesAccessories = Guid.Parse("A1000000-0000-0000-0000-000000000012");
        var audioHeadphones = Guid.Parse("A1000000-0000-0000-0000-000000000013");
        var gaming = Guid.Parse("A1000000-0000-0000-0000-000000000014");

        var art = Guid.Parse("A1000000-0000-0000-0000-000000000021");
        var collectibles = Guid.Parse("A1000000-0000-0000-0000-000000000022");

        var cars = Guid.Parse("A1000000-0000-0000-0000-000000000031");
        var motorcycles = Guid.Parse("A1000000-0000-0000-0000-000000000032");
        var bicycles = Guid.Parse("A1000000-0000-0000-0000-000000000033");

        var furniture = Guid.Parse("A1000000-0000-0000-0000-000000000041");
        var appliances = Guid.Parse("A1000000-0000-0000-0000-000000000042");
        var toolsEquipment = Guid.Parse("A1000000-0000-0000-0000-000000000043");

        builder.HasData(
            new { Id = electronics, Name = "Electronics", Slug = "electronics", ParentCategoryId = (Guid?)null },
            new { Id = computers, Name = "Computers", Slug = "computers", ParentCategoryId = electronics },
            new { Id = smartphonesAccessories, Name = "Smartphones & Accessories", Slug = "smartphones-accessories", ParentCategoryId = electronics },
            new { Id = audioHeadphones, Name = "Audio & Headphones", Slug = "audio-headphones", ParentCategoryId = electronics },
            new { Id = gaming, Name = "Gaming", Slug = "gaming", ParentCategoryId = electronics },

            new { Id = collectiblesArt, Name = "Collectibles & Art", Slug = "collectibles-art", ParentCategoryId = (Guid?)null },
            new { Id = art, Name = "Art", Slug = "art", ParentCategoryId = collectiblesArt },
            new { Id = collectibles, Name = "Collectibles", Slug = "collectibles", ParentCategoryId = collectiblesArt },

            new { Id = vehicles, Name = "Vehicles", Slug = "vehicles", ParentCategoryId = (Guid?)null },
            new { Id = cars, Name = "Cars", Slug = "cars", ParentCategoryId = vehicles },
            new { Id = motorcycles, Name = "Motorcycles", Slug = "motorcycles", ParentCategoryId = vehicles },
            new { Id = bicycles, Name = "Bicycles", Slug = "bicycles", ParentCategoryId = vehicles },

            new { Id = homeGarden, Name = "Home & Garden", Slug = "home-garden", ParentCategoryId = (Guid?)null },
            new { Id = furniture, Name = "Furniture", Slug = "furniture", ParentCategoryId = homeGarden },
            new { Id = appliances, Name = "Appliances", Slug = "appliances", ParentCategoryId = homeGarden },
            new { Id = toolsEquipment, Name = "Tools & Equipment", Slug = "tools-equipment", ParentCategoryId = homeGarden }
        );
    }
}
