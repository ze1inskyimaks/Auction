using System.Text;
using System.Text.Json;
using Auction.BL.Interface;
using Auction.BL.Model.Result;
using Auction.Data.Model;
using Microsoft.Extensions.Caching.Distributed;

namespace Auction.BL.Implementation;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IAuctionLotService _lotService;

    public CacheService(IDistributedCache cache, IAuctionLotService lotService)
    {
        _cache = cache;
        _lotService = lotService;
    }
    
    public async Task<Result> CacheActiveAuctionLotsAsync()
    {
        try
        {
            var activeList = _lotService.GetListOfActiveAuctionLots();

            // Серіалізуємо список аукціонів у JSON
            var serializedData = JsonSerializer.Serialize(activeList);

            // Записуємо серіалізовані дані в кеш
            await _cache.SetAsync("activeAuctionLots", Encoding.UTF8.GetBytes(serializedData),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // або вказати інший час життя
                });
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.ToString());
        }
    }
    public async Task<List<AuctionLot>?> GetCachedActiveAuctionLotsAsync()
    {
        try
        {
            var cachedData = await _cache.GetAsync("activeAuctionLots");

            if (cachedData == null)
                return null;

            var serializedData = Encoding.UTF8.GetString(cachedData);
            return JsonSerializer.Deserialize<List<AuctionLot>>(serializedData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}