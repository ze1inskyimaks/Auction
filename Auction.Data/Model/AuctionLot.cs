namespace Auction.Data.Model;

public class AuctionLot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string Description { get; set; } = String.Empty;
    public DateTime StartTime { get; set; } = DateTime.Now.AddHours(1);
    
    public Guid OwnerId { get; set; }
    public Account? OwnerAccount { get; set; }

    public Guid? CurrentWinnerId { get; set; }
    public double CurrentPrice { get; set; } = 0;
    public DateTime LastBitTime { get; set; } = DateTime.MaxValue; 
    
    public List<Guid>? AuctionHistoryId { get; set; }
    public List<AuctionHistory>? AuctionHistories { get; set; }
    
    public double StartPrice { get; set; } = 0;
    public double EndPrice { get; set; } = 0;
    public string? WinnerId { get; set; }
    public Account? WinnerAccount { get; set; }

    public Status Status { get; set; } = Status.Active;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}