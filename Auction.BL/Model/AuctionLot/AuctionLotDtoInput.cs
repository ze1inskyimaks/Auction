namespace Auction.BL.Model.AuctionLot;

public class AuctionLotDtoInput
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    public DateTime StartTime { get; set; }
    public double StartPrice { get; set; } = 0;
}