using Auction.BL.Model.Result;
using Auction.Data.Model;

namespace Auction.BL.Interface;

public interface ICacheService
{
    public Task<Result> CacheActiveAuctionLotsAsync();
    public Task<List<AuctionLot>?> GetCachedActiveAuctionLotsAsync();
}