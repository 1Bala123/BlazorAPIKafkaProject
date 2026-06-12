

using  System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Serilog;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(
        string key,
        T value);

    Task RemoveAsync(string key);
}

namespace CatalogService.Infrastructure.Rediscache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisCacheSettings _settings;

        public CacheService(
            IDistributedCache cache,
            IOptions<RedisCacheSettings> settings)
        {
            _cache = cache;
            _settings = settings.Value;
        }   
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                Log.Information("Redis cache GetTasync Value reading Starts");
                var data = await _cache.GetStringAsync(key);

                if (string.IsNullOrWhiteSpace(data))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(data);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at GetTAsyc Redis Cache reading");
                return default;
            }
            finally
            {
                Log.Information("Redis cache GetTasync Value reading Ends");   
            }

        }

        public async Task SetAsync<T>(string key,T value)
        {
            try{

                Log.Information("Redis cache SetAsync Value reading Starts");   
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(
                            _settings.DefaultExpirationMinutes)
                };
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at SetAsync Redis Cache reading");
            }
            finally
            {
                Log.Information("Redis cache SetAsync Value reading Ends");   
            }
        }

        public async Task RemoveAsync(string key)
        {
            try{

                Log.Information("Redis cache RemoveAsync Starts");  
                await _cache.RemoveAsync(key);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at RemoveAsync Redis Cache reading");
            }
            finally
            {
                Log.Information("Redis cache RemoveAsync Ends");   
            }
        }
    }
}