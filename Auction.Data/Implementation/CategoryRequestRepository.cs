using Auction.Data.Interface;
using Auction.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace Auction.Data.Implementation;

public class CategoryRequestRepository : ICategoryRequestRepository
{
    private readonly AppDbContext _context;

    public CategoryRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryRequest> Create(CategoryRequest request)
    {
        await _context.CategoryRequests.AddAsync(request);
        await _context.SaveChangesAsync();
        return request;
    }

    public async Task<CategoryRequest?> GetById(Guid id)
    {
        return await _context.CategoryRequests
            .Include(r => r.RequestedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<CategoryRequest?> GetPendingByName(string name)
    {
        var normalized = name.Trim().ToLower();
        return await _context.CategoryRequests
            .FirstOrDefaultAsync(r =>
                r.Status == CategoryRequestStatus.Pending &&
                r.Name.ToLower() == normalized);
    }

    public async Task<List<CategoryRequest>> GetAll()
    {
        return await _context.CategoryRequests
            .Include(r => r.RequestedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<CategoryRequest> Update(CategoryRequest request)
    {
        _context.CategoryRequests.Update(request);
        await _context.SaveChangesAsync();
        return request;
    }
}
