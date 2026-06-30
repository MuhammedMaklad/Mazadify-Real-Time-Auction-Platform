namespace AuctionPlatform.Domain.Entities;

public class AuctionCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }

    // Navigation
    public AuctionCategory? ParentCategory { get; set; }
    public ICollection<AuctionCategory> SubCategories { get; set; } = new List<AuctionCategory>();
    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}
