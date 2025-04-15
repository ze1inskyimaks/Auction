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
    
    public async Task CreateLot(AuctionLot lot)
    {
        await _context.AuctionLots.AddAsync(lot);
        await _context.SaveChangesAsync();
    }

    public async Task ChangeLot(AuctionLot lot)
    {
        _context.Update(lot);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLot(AuctionLot lot)
    {
        _context.Remove(lot);
        await _context.SaveChangesAsync();
    }

    public async Task<AuctionLot?> GetLot(Guid id)
    {
        return await _context.AuctionLots.FindAsync(id);
    }
}