namespace Auction.Data.Model;

public class AuctionLotImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LotId { get; set; }
    public AuctionLot Lot { get; set; } = null!;
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
