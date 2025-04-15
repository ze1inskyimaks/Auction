using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionHistoryRepository
{
    public Task CreateHistoryLog(AuctionHistory lot);
    public Task DeleteHistoryLog(AuctionHistory lot);

    public Task<AuctionHistory?> GetHistoryLog(Guid id);
}