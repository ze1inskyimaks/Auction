using System.ComponentModel.DataAnnotations;

namespace Auction.Data.Model;

public class AuctionLot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    public string? LinkToImage { get; set; }
    public AuctionLotImage? Image { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow.AddHours(1);
    public string JobId { get; set; } = string.Empty;
    
    [Required]
    public string OwnerId { get; set; }
    public Account? OwnerAccount { get; set; }

    public Guid? CategoryId { get; set; }
    public AuctionCategory? Category { get; set; }

    public Guid? CurrentWinnerId { get; set; }
    public double CurrentPrice { get; set; }
    public DateTime LastBitTime { get; set; } = DateTime.MaxValue;
    
    public List<Guid>? AuctionHistoryId { get; set; }
    public List<AuctionHistory>? AuctionHistories { get; set; }
    
    [Required]
    public double StartPrice { get; set; }
    public double EndPrice { get; set; }
    public string? WinnerId { get; set; }
    public Account? WinnerAccount { get; set; }

    public Status Status { get; set; } = Status.Active;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
