using Auction.Data.Model;

namespace Auction.BL.Interface;

public interface ICacheService
{
    public Task CacheActiveAuctionLotsAsync();
    public Task<List<AuctionLot>?> GetCachedActiveAuctionLotsAsync();
}