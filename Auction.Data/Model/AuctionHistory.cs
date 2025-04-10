namespace Auction.Data.Model;

public class AuctionHistory
{
    public Guid Id { get; set; }
    
    public Guid LotId { get; set; }
    public required AuctionLot AuctionLot { get; set; }
    
    public int HistoryNumber { get; set; }

    public Guid BidderId { get; set; }
    public required Account Bidder { get; set; }

    public double BidAmount { get; set; }
    public DateTime BidTime { get; set; } = DateTime.Now;
    
    public Guid? PreviousBidderId { get; set; }
    public Account? PreviousBidder { get; set; }
    
    public Guid? NewBidderId { get; set; }
    public Account? NewBidder { get; set; }

    public BiddenStatus Status { get; set; } = BiddenStatus.Accepted;
}