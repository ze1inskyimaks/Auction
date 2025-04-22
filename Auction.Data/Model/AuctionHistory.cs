namespace Auction.Data.Model;

public class AuctionHistory
{
    public Guid Id { get; set; }
    
    public Guid LotId { get; set; }
    public required AuctionLot AuctionLot { get; set; }

    public int? HistoryNumber { get; set; } = 0;

    public Guid BidderId { get; set; }
    public double BidAmount { get; set; }
    public DateTime BidTime { get; set; } = DateTime.Now;
}