using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data.Implementation;

public class AuctionCategoryRepository : IAuctionCategoryRepository
{
    private readonly AppDbContext _context;

    public AuctionCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuctionCategory>> GetActiveCategories()
    {
        return await _context.AuctionCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<AuctionCategory?> GetById(Guid id)
    {
        return await _context.AuctionCategories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<AuctionCategory?> GetByName(string name)
    {
        var normalized = name.Trim();
        return await _context.AuctionCategories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized.ToLower());
    }

    public async Task<AuctionCategory> Create(AuctionCategory category)
    {
        await _context.AuctionCategories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }
}
