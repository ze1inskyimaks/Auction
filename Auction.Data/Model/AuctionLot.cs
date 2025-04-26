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
    public DateTime StartTime { get; set; } = DateTime.Now.AddHours(1);
    public string JobId { get; set; } = String.Empty;
    
    [Required]
    public string OwnerId { get; set; }
    public Account? OwnerAccount { get; set; }

    public Guid? CurrentWinnerId { get; set; }
    public double CurrentPrice { get; set; } = 0;
    public DateTime LastBitTime { get; set; } = DateTime.MaxValue; 
    
    public List<Guid>? AuctionHistoryId { get; set; }
    public List<AuctionHistory>? AuctionHistories { get; set; }
    
    [Required]
    public double StartPrice { get; set; } = 0;
    public double EndPrice { get; set; } = 0;
    public string? WinnerId { get; set; }
    public Account? WinnerAccount { get; set; }

    public Status Status { get; set; } = Status.Active;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}