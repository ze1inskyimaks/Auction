using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionLotRepository
{
    public Task<AuctionLot> CreateLot(AuctionLot lot);
    public Task<AuctionLot> ChangeLot(AuctionLot lot);
    public Task<AuctionLot> DeleteLot(AuctionLot lot);

    public Task<AuctionLot?> GetLot(Guid id);
    public List<AuctionLot>? GetActiveLot();
}