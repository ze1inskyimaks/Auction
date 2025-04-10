using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionHistoryRepository
{
    public void CreateHistoryLog();
    public void DeleteHistoryLog();

    public AuctionLot GetHistoryLog(Guid id);
}