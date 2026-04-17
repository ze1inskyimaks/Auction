using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionHistoryRepository
{
    public Task<AuctionHistory> CreateHistoryLog(AuctionHistory log);
    public Task<AuctionHistory> DeleteHistoryLog(AuctionHistory log);

    public Task<AuctionHistory?> GetHistoryLog(Guid id);
    public Task<List<AuctionHistory>> GetHistoryLogsByLotId(Guid lotId);
    public Task<List<AuctionHistory>> GetHistoryLogsByBidderId(Guid bidderId);
    public Task<int?> GetLastHistoryNumberByLotId(Guid lotId);
}
