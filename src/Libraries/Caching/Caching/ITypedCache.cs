using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Dgt.Caching
{
    public interface ITypedCache
    {
        [return: NotNull]
        public Task SetRecordAsync<T>(
            [NotNull] string key,
            [NotNull] T value,
            TimeSpan? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null
        );

        [return: NotNull]
        public Task<T?> GetRecordAsync<T>([NotNull] string key);
    }
}