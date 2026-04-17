using System.ComponentModel.DataAnnotations;

namespace Auction.Data.Model;

public class CategoryRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public required string Name { get; set; }

    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    public required string RequestedById { get; set; }
    public Account? RequestedBy { get; set; }

    public CategoryRequestStatus Status { get; set; } = CategoryRequestStatus.Pending;
    public string? AdminComment { get; set; }

    public string? ReviewedById { get; set; }
    public Account? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Guid? CategoryId { get; set; }
    public AuctionCategory? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
