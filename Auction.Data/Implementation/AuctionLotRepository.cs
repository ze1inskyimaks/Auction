using Auction.Data.Interface;
using Auction.Data.Model;

namespace Auction.Data.Implementation;

public class AuctionLotRepository : IAuctionLotRepository
{
    private readonly AppDbContext _context;

    public AuctionLotRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<AuctionLot> CreateLot(AuctionLot lot)
    {
        await _context.AuctionLots.AddAsync(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    public async Task<AuctionLot> ChangeLot(AuctionLot lot)
    {
        _context.Update(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    public async Task<AuctionLot> DeleteLot(AuctionLot lot)
    {
        _context.Remove(lot);
        await _context.SaveChangesAsync();
        return lot;
    }

    public async Task<AuctionLot?> GetLot(Guid id)
    {
        return await _context.AuctionLots.FindAsync(id);
    }

    public List<AuctionLot> GetActiveLot()
    {
        var list = _context.AuctionLots
            .Where(e => e.Status == Status.Active || e.Status == Status.Open)
            .ToList();
        return list;
    }
}