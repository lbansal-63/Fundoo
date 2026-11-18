using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Middleware.Services.Cache
{
    public class RedisCacheService : ICacheService
    {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // Redis down ho to exception nahi phenkte, bas cache miss maante hain
        public T? Get<T>(string key)
        {
            try
            {
                var json = _cache.GetString(key);
                return json == null ? default : JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache read failed for {Key}", key);
                return default;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? ttl = null)
        {
            try
            {
                _cache.SetString(key, JsonSerializer.Serialize(value), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache write failed for {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try { _cache.Remove(key); }
            catch (Exception ex) { _logger.LogWarning(ex, "Cache remove failed for {Key}", key); }
        }
    }
}