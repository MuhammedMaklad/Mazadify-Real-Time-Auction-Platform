using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence;

public class DbSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AppDbContext _context;

    public DbSeeder(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        await SeedRolesAsync();
        var users = await SeedUsersAsync();
        var auctions = SeedAuctions(users);
        _context.Auctions.AddRange(auctions);

        var items = SeedAuctionItems(auctions);
        _context.AuctionItems.AddRange(items);

        var images = SeedAuctionItemImages(items);
        _context.AuctionItemImages.AddRange(images);

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Admin", "Seller", "Bidder" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }
    }

    private async Task<List<User>> SeedUsersAsync()
    {
        var users = new List<User>
        {
            new()
            {
                Id = Guid.Parse("B1000000-0000-0000-0000-000000000001"),
                UserName = "admin",
                Email = "admin@mazadify.com",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("B1000000-0000-0000-0000-000000000002"),
                UserName = "seller1",
                Email = "seller1@mazadify.com",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("B1000000-0000-0000-0000-000000000003"),
                UserName = "seller2",
                Email = "seller2@mazadify.com",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _userManager.CreateAsync(users[0], "Admin@123");
        await _userManager.AddToRoleAsync(users[0], "Admin");
        await _userManager.AddToRoleAsync(users[0], "Seller");

        await _userManager.CreateAsync(users[1], "Seller@123");
        await _userManager.AddToRoleAsync(users[1], "Seller");

        await _userManager.CreateAsync(users[2], "Seller@123");
        await _userManager.AddToRoleAsync(users[2], "Seller");

        return users;
    }

    private static List<Auction> SeedAuctions(List<User> users)
    {
        var now = DateTime.UtcNow;
        var admin = users[0];
        var seller1 = users[1];
        var seller2 = users[2];

        return
        [
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000001"),
                SellerId = seller1.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000022"),
                Title = "Vintage Camera Collection",
                Description = "A rare collection of vintage film cameras from the 1960s and 1970s, including a Leica M3 and a Hasselblad 500C.",
                StartingPrice = 500.00m,
                ReservePrice = 800.00m,
                CurrentHighestBid = 0,
                BidIncrement = 25.00m,
                StartTime = now.AddDays(7),
                EndTime = now.AddDays(14),
                Status = AuctionStatus.Draft,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000002"),
                SellerId = seller1.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000011"),
                Title = "Gaming Laptop Pro X",
                Description = "High-performance gaming laptop with RTX 4080, 32GB RAM, 1TB SSD. Used for 3 months, excellent condition.",
                StartingPrice = 1200.00m,
                ReservePrice = 1500.00m,
                CurrentHighestBid = 0,
                BidIncrement = 50.00m,
                StartTime = now.AddDays(2),
                EndTime = now.AddDays(9),
                Status = AuctionStatus.Scheduled,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000003"),
                SellerId = seller2.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000013"),
                Title = "Wireless Noise-Canceling Headphones",
                Description = "Premium over-ear headphones with active noise cancellation. 40-hour battery life. Like new, used twice.",
                StartingPrice = 150.00m,
                ReservePrice = 200.00m,
                CurrentHighestBid = 175.00m,
                BidIncrement = 10.00m,
                StartTime = now.AddHours(-1),
                EndTime = now.AddDays(6).AddHours(-1),
                Status = AuctionStatus.Live,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000004"),
                SellerId = seller1.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000041"),
                Title = "Antique Wooden Desk",
                Description = "Beautiful hand-carved mahogany desk from the early 1900s. Minor wear consistent with age. Solid construction.",
                StartingPrice = 800.00m,
                ReservePrice = 1200.00m,
                CurrentHighestBid = 850.00m,
                BidIncrement = 50.00m,
                StartTime = now.AddHours(-6),
                EndTime = now.AddDays(5).AddHours(-6),
                Status = AuctionStatus.Live,
                DeliveryType = DeliveryType.Pickup,
                DeliveryNotes = "Item is located in Cairo. Buyer must arrange pickup within 14 days of auction end.",
                CreatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000005"),
                SellerId = seller2.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000031"),
                Title = "Classic Mustang 1967",
                Description = "Fully restored Ford Mustang 1967 GT. New engine, new tires, new paint job. A true collector's item.",
                StartingPrice = 25000.00m,
                ReservePrice = 35000.00m,
                CurrentHighestBid = 28000.00m,
                BidIncrement = 1000.00m,
                StartTime = now.AddDays(-7),
                EndTime = now.AddDays(-1),
                Status = AuctionStatus.Ended,
                DeliveryType = DeliveryType.Pickup,
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000006"),
                SellerId = seller1.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000012"),
                Title = "Latest Smartphone Bundle",
                Description = "Flagship smartphone with accessories: case, screen protector, wireless charger, and extra cable.",
                StartingPrice = 600.00m,
                ReservePrice = 900.00m,
                CurrentHighestBid = 650.00m,
                BidIncrement = 25.00m,
                StartTime = now.AddDays(-7),
                EndTime = now.AddDays(-1),
                Status = AuctionStatus.ReserveNotMet,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000007"),
                SellerId = seller2.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000014"),
                Title = "PS5 Pro + 5 Games Bundle",
                Description = "PlayStation 5 Pro console in perfect condition with 5 popular games including the latest releases.",
                StartingPrice = 400.00m,
                ReservePrice = 600.00m,
                CurrentHighestBid = 450.00m,
                BidIncrement = 20.00m,
                StartTime = now.AddHours(-2),
                EndTime = now.AddDays(5).AddHours(-2),
                Status = AuctionStatus.Live,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Id = Guid.Parse("C1000000-0000-0000-0000-000000000008"),
                SellerId = admin.Id,
                CategoryId = Guid.Parse("A1000000-0000-0000-0000-000000000043"),
                Title = "Professional Tool Set",
                Description = "Complete 200-piece professional tool set. Includes wrenches, screwdrivers, pliers, socket set, and more.",
                StartingPrice = 200.00m,
                ReservePrice = 0,
                CurrentHighestBid = 0,
                BidIncrement = 10.00m,
                StartTime = now.AddDays(1),
                EndTime = now.AddDays(8),
                Status = AuctionStatus.Scheduled,
                DeliveryType = DeliveryType.Shipping,
                CreatedAt = now
            }
        ];
    }

    private static List<AuctionItem> SeedAuctionItems(List<Auction> auctions)
    {
        var items = new List<AuctionItem>();
        var now = DateTime.UtcNow;

        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000001"),
            AuctionId = auctions[0].Id,
            Name = "Leica M3 (1958)",
            Description = "Classic Leica M3 rangefinder camera with 50mm f/2 Summicron lens. Fully functional.",
            Condition = "Used",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000002"),
            AuctionId = auctions[0].Id,
            Name = "Hasselblad 500C (1972)",
            Description = "Medium format camera with 80mm f/2.8 Carl Zeiss lens. Includes 12 exposure back.",
            Condition = "Used",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000003"),
            AuctionId = auctions[1].Id,
            Name = "Gaming Laptop Pro X - 16\"",
            Description = "RTX 4080, Intel i9, 32GB DDR5, 1TB NVMe SSD, 165Hz display.",
            Condition = "Used",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000004"),
            AuctionId = auctions[2].Id,
            Name = "ANC Headphones - Black",
            Description = "Premium noise-canceling headphones with carrying case, USB-C cable, and 3.5mm audio cable.",
            Condition = "Like New",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000005"),
            AuctionId = auctions[3].Id,
            Name = "Mahogany Writing Desk",
            Description = "Hand-carved mahogany desk with 3 drawers. Dimensions: 150cm x 75cm x 80cm.",
            Condition = "Used",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000006"),
            AuctionId = auctions[3].Id,
            Name = "Matching Chair",
            Description = "Original matching mahogany chair with velvet upholstery. Some wear on armrests.",
            Condition = "Used",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000007"),
            AuctionId = auctions[4].Id,
            Name = "Ford Mustang GT 1967",
            Description = "Restored classic Mustang. 289 V8 engine, 4-speed manual transmission. Red exterior, black interior.",
            Condition = "Refurbished",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000008"),
            AuctionId = auctions[5].Id,
            Name = "Smartphone - 256GB",
            Description = "Latest model, 256GB storage, 12GB RAM. Includes original box and charger.",
            Condition = "Like New",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000009"),
            AuctionId = auctions[5].Id,
            Name = "Premium Accessories Bundle",
            Description = "Includes leather case, tempered glass, wireless charger, and USB-C cable set.",
            Condition = "New",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000010"),
            AuctionId = auctions[6].Id,
            Name = "PS5 Pro Console",
            Description = "PS5 Pro with 2TB SSD, 2 controllers, and original packaging.",
            Condition = "Like New",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000011"),
            AuctionId = auctions[6].Id,
            Name = "Game Bundle - 5 Titles",
            Description = "Includes Spider-Man 2, God of War Ragnarok, Final Fantasy XVI, Call of Duty, and Elden Ring.",
            Condition = "New",
            CreatedAt = now
        });
        items.Add(new()
        {
            Id = Guid.Parse("D1000000-0000-0000-0000-000000000012"),
            AuctionId = auctions[7].Id,
            Name = "Mechanic Tool Set - 200pc",
            Description = "Complete set with metric and SAE sockets, wrenches, screwdrivers, pliers, hex keys, and storage case.",
            Condition = "New",
            CreatedAt = now
        });

        return items;
    }

    private static List<AuctionItemImage> SeedAuctionItemImages(List<AuctionItem> items)
    {
        var images = new List<AuctionItemImage>();

        foreach (var item in items)
        {
            images.Add(new()
            {
                Id = Guid.NewGuid(),
                AuctionItemId = item.Id,
                ImageUrl = $"https://placehold.co/800x600?text={Uri.EscapeDataString(item.Name)}",
                IsPrimary = true,
            });

            images.Add(new()
            {
                Id = Guid.NewGuid(),
                AuctionItemId = item.Id,
                ImageUrl = $"https://placehold.co/800x600?text={Uri.EscapeDataString(item.Name)}+2",
                IsPrimary = false,
            });
        }

        return images;
    }
}
