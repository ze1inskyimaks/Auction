using System.Text;
using System.Text.Json;
using Auction.BL.Interface;
using Auction.BL.Model.AuctionLot;
using Auction.BL.Model.Result;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Auction.BL.Implementation;

public class CacheService : ICacheService
{
    private const string ActiveLotsCacheKey = "activeAuctionLots";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RedisCooldown = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan RedisOperationTimeout = TimeSpan.FromMilliseconds(500);

    private readonly IDistributedCache _cache;
    private readonly IMemoryCache _memoryCache;
    private readonly IAuctionLotService _lotService;
    private readonly ILogger<CacheService> _logger;
    private DateTime _redisRetryAtUtc = DateTime.MinValue;

    public CacheService(
        IDistributedCache cache,
        IMemoryCache memoryCache,
        IAuctionLotService lotService,
        ILogger<CacheService> logger)
    {
        _cache = cache;
        _memoryCache = memoryCache;
        _lotService = lotService;
        _logger = logger;
    }
    
    public async Task<Result> CacheActiveAuctionLotsAsync()
    {
        try
        {
            var activeList = _lotService.GetListOfActiveAuctionLots();
            var serializedData = JsonSerializer.Serialize(activeList);
            _memoryCache.Set(ActiveLotsCacheKey, activeList, CacheTtl);

            if (!CanUseRedis())
            {
                return Result.Success();
            }

            using var cts = new CancellationTokenSource(RedisOperationTimeout);
            await _cache.SetAsync(
                ActiveLotsCacheKey,
                Encoding.UTF8.GetBytes(serializedData),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheTtl
                },
                cts.Token);

            return Result.Success();
        }
        catch (Exception e)
        {
            MarkRedisUnavailable(e);
            return Result.Success();
        }
    }

    public async Task<List<AuctionLotDtoOutput>?> GetCachedActiveAuctionLotsAsync()
    {
        if (_memoryCache.TryGetValue(ActiveLotsCacheKey, out List<AuctionLotDtoOutput>? localCached) &&
            localCached is not null)
        {
            return localCached;
        }

        if (!CanUseRedis())
        {
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(RedisOperationTimeout);
            var cachedData = await _cache.GetAsync(ActiveLotsCacheKey, cts.Token);
            if (cachedData == null)
            {
                return null;
            }

            var serializedData = Encoding.UTF8.GetString(cachedData);
            var deserialized = JsonSerializer.Deserialize<List<AuctionLotDtoOutput>>(serializedData);
            if (deserialized is not null)
            {
                _memoryCache.Set(ActiveLotsCacheKey, deserialized, CacheTtl);
            }

            return deserialized;
        }
        catch (Exception e)
        {
            MarkRedisUnavailable(e);
            return null;
        }
    }

    private bool CanUseRedis()
    {
        return DateTime.UtcNow >= _redisRetryAtUtc;
    }

    private void MarkRedisUnavailable(Exception exception)
    {
        _redisRetryAtUtc = DateTime.UtcNow.Add(RedisCooldown);
        _logger.LogWarning(exception, "Redis is unavailable. Switching to memory fallback until {RetryAtUtc}", _redisRetryAtUtc);
    }
}
