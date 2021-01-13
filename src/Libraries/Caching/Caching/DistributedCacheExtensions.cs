using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Caching.Distributed;

namespace Dgt.Caching
{
    // TODO Cancellation tokens!
    public static class DistributedCacheExtensions
    {
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
    }
}