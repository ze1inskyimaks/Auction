using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionLotRepository
{
    public Task CreateLot(AuctionLot lot);
    public Task ChangeLot(AuctionLot lot);
    public Task DeleteLot(AuctionLot lot);

    public Task<AuctionLot?> GetLot(Guid id);
}