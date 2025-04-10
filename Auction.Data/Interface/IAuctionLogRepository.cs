using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionLogRepository
{
    public void CreateLot();
    public void ChangeLot();
    public void DeleteLot();

    public AuctionLot GetLot(Guid id);
}