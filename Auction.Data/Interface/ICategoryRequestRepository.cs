using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface ICategoryRequestRepository
{
    Task<CategoryRequest> Create(CategoryRequest request);
    Task<CategoryRequest?> GetById(Guid id);
    Task<CategoryRequest?> GetPendingByName(string name);
    Task<List<CategoryRequest>> GetAll();
    Task<CategoryRequest> Update(CategoryRequest request);
}
