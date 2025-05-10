using System.Text;
using System.Text.Json;
using Auction.BL.Interface;
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
    
    public async Task CacheActiveAuctionLotsAsync()
    {
        var activeList = _lotService.GetListOfActiveAuctionLots();

        // Серіалізуємо список аукціонів у JSON
        var serializedData = JsonSerializer.Serialize(activeList);

        // Записуємо серіалізовані дані в кеш
        await _cache.SetAsync("activeAuctionLots", Encoding.UTF8.GetBytes(serializedData), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // або вказати інший час життя
        });
    }
    public async Task<List<AuctionLot>?> GetCachedActiveAuctionLotsAsync()
    {
        // Отримуємо байтові дані з кешу
        var cachedData = await _cache.GetAsync("activeAuctionLots");

        if (cachedData == null)
            return null;

        // Десеріалізуємо дані назад у список аукціонів
        var serializedData = Encoding.UTF8.GetString(cachedData);
        return JsonSerializer.Deserialize<List<AuctionLot>>(serializedData);
    }
}