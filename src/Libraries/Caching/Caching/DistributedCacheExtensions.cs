using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Caching.Distributed;

namespace Dgt.Caching
{
    // TODO Cancellation tokens!
    // ENHANCE Introduce an adapter in this library so other libraries can unit test against it more
    //         easily; other libraries cannot test against these extension methods
    public static class DistributedCacheExtensions
    {
        // TODO If you implement TryGet you should probably have TrySet
        public static async Task SetRecordAsync<T>
        (
            [NotNull] this IDistributedCache cache,
            [NotNull] string key,
            [NotNull] T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null
        )
        {
            _ = cache.WhenNotNull(nameof(cache));
            _ = key.WhenNotMissing(nameof(key));
            _ = value.WhenNotNull(nameof(value));

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromSeconds(60),
                SlidingExpiration = slidingExpiration
            };
            var json = JsonSerializer.Serialize(value);

            await cache.SetStringAsync(key, json, options);
        }
        
        // ENHANCE Implement the TryGet pattern so this implementation can throw on missing
        public static async Task<T?> GetRecordAsync<T>([NotNull] this IDistributedCache cache, [NotNull] string key)
        {
            _ = cache.WhenNotNull(nameof(cache));
            _ = key.WhenNotMissing(nameof(key));
            
            var json = await cache.GetStringAsync(key);
            
            return json is null
                ? default
                : JsonSerializer.Deserialize<T>(json);
        }
    }
}