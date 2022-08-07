using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Web1
{
    public class Cache
    {
        private readonly IDistributedCache _cache;

        public Cache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<Item> Get(string key)
        {
            var cacheResponse = await _cache.GetStringAsync(key);
            if (cacheResponse is null)
            {
                return null;
            }

            var returnVal = JsonConvert.DeserializeObject<Item>(cacheResponse);
            return returnVal;
        }

        public async Task Set(string key, Item data)
        {
            var response = JsonConvert.SerializeObject(data);
            await _cache.SetStringAsync(key, response);
        }

        public async Task Clear(string key)
        {
            await _cache.RemoveAsync(key);
        }

    }
}
