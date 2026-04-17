using System.ComponentModel.DataAnnotations;

namespace Auction.Data.Model;

public class AuctionCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public required string Name { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<AuctionLot>? Lots { get; set; } = new();
}
