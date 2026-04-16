using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.EntityFrameworkCore;

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
        return await _context.AuctionLots
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public List<AuctionLot> GetActiveLot()
    {
        var list = _context.AuctionLots
            .Include(l => l.Category)
            .Where(e => e.Status == Status.Active || e.Status == Status.Open)
            .ToList();
        return list;
    }

    public List<AuctionLot> GetArchivedLot()
    {
        var list = _context.AuctionLots
            .Include(l => l.Category)
            .Where(e => e.Status == Status.Sold || e.Status == Status.Cancelled || e.Status == Status.Delivered)
            .OrderByDescending(e => e.UpdatedAt)
            .ThenByDescending(e => e.CreatedAt)
            .ToList();
        return list;
    }

    public List<AuctionLot> GetWonLotsByWinnerId(string winnerId)
    {
        var list = _context.AuctionLots
            .Include(l => l.Category)
            .Where(e => (e.Status == Status.Sold || e.Status == Status.Delivered) && e.WinnerId == winnerId)
            .OrderByDescending(e => e.UpdatedAt)
            .ThenByDescending(e => e.CreatedAt)
            .ToList();
        return list;
    }
}
