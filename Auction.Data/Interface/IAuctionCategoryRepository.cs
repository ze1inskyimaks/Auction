using Auction.Data.Model;

namespace Auction.Data.Interface;

public interface IAuctionCategoryRepository
{
    Task<List<AuctionCategory>> GetActiveCategories();
    Task<AuctionCategory?> GetById(Guid id);
    Task<AuctionCategory?> GetByName(string name);
    Task<AuctionCategory> Create(AuctionCategory category);
}
