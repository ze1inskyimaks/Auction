using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data.Implementation;

public class AuctionHistoryRepository : IAuctionHistoryRepository
{
    private readonly AppDbContext _context;

    public AuctionHistoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<AuctionHistory> CreateHistoryLog(AuctionHistory log)
    {
        await _context.AuctionHistories.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<AuctionHistory> DeleteHistoryLog(AuctionHistory log)
    {
        _context.AuctionHistories.Remove(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<AuctionHistory?> GetHistoryLog(Guid id)
    {
        return await _context.AuctionHistories.FindAsync(id);
    }

    public async Task<int?> GetLastHistoryNumberByLotId(Guid lotId)
    {
        var latestHistory = await _context.AuctionHistories
            .Where(h => h.LotId == lotId)
            .OrderByDescending(h => h.HistoryNumber)
            .FirstOrDefaultAsync();
        return latestHistory?.HistoryNumber;
    }
}