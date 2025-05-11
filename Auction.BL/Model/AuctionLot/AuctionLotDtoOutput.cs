using Auction.Data.Model;

namespace Auction.BL.Model.AuctionLot;

public class AuctionLotDtoOutput
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? LinkToImage { get; set; }

    public DateTime StartTime { get; set; }
    
    public string OwnerId { get; set; }

    public Guid? CurrentWinnerId { get; set; }
    public double CurrentPrice { get; set; }
    public DateTime LastBitTime { get; set; }

    public List<Guid>? AuctionHistoryId { get; set; }
    
    public double StartPrice { get; set; } = 0;
    public double EndPrice { get; set; } = 0;
    public string? WinnerId { get; set; }

    public Status Status { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}