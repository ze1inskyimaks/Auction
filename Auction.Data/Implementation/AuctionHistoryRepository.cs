using Auction.Data.Interface;
using Auction.Data.Model;

namespace Auction.Data.Implementation;

public class AuctionHistoryRepository : IAuctionHistoryRepository
{
    private readonly AppDbContext _context;

    public AuctionHistoryRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateHistoryLog(AuctionHistory lot)
    {
        await _context.AuctionHistories.AddAsync(lot);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteHistoryLog(AuctionHistory lot)
    {
        _context.AuctionHistories.Remove(lot);
        await _context.SaveChangesAsync();
    }

    public async Task<AuctionHistory?> GetHistoryLog(Guid id)
    {
        return await _context.AuctionHistories.FindAsync(id);
    }
}