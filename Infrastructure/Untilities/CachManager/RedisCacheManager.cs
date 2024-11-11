using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Untilities.CachManager
{
    public class RedisCacheManager : IRedisCacheManager
    {
        private readonly IDistributedCache _cache;

        public RedisCacheManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetItemAsync<T>(string key, T item, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(60) // Default cache time
            };

            var jsonData = JsonSerializer.Serialize(item);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            var jsonData = await _cache.GetStringAsync(key);
            return jsonData is null ? default(T) : JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}
