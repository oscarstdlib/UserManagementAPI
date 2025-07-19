using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace UserManagementAPI.Services
{
    public class CacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public T? Get<T>(string key)
        {
            try
            {
                if (_cache.TryGetValue(key, out T? value))
                {
                    _logger.LogInformation("Cache hit for key: {Key}", key);
                    return value;
                }
                
                _logger.LogInformation("Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return default;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration: 1 hour
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                }

                _cache.Set(key, value, options);
                _logger.LogInformation("Value cached for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
                _logger.LogInformation("Cache entry removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cachedValue = Get<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            Set(key, value, expiration);
            return value;
        }

        public string GenerateKey(string prefix, params object[] parameters)
        {
            var keyParts = new List<string> { prefix };
            keyParts.AddRange(parameters.Select(p => p?.ToString() ?? "null"));
            return string.Join(":", keyParts);
        }
    }
} 