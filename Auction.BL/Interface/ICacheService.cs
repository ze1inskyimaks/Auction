using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Result;

namespace Auction.BL.Interface;

public interface ICacheService
{
    public Task<Result> CacheActiveAuctionLotsAsync();
    public Task<List<AuctionLotDtoOutput>?> GetCachedActiveAuctionLotsAsync();
}
