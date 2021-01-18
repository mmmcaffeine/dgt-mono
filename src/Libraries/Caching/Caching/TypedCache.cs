using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Caching.Distributed;

namespace Dgt.Caching
{
    public class TypedCache : ITypedCache
    {
        private readonly IDistributedCache _cache;

        public TypedCache(IDistributedCache cache)
        {
            _cache = cache.WhenNotNull(nameof(cache));
        }

        [return: NotNull]
        public Task SetRecordAsync<T>
        (
            [NotNull] string key,
            [NotNull] T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null
        )
        {
            return _cache.SetRecordAsync(key, value, absoluteExpiration, slidingExpiration);
        }

        [return: NotNull]
        public Task<T?> GetRecordAsync<T>([NotNull] string key)
        {
            return _cache.GetRecordAsync<T>(key);
        }
    }
}