using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.Distributed;
using Polly.Caching.Serialization.Json;
using Xunit;
using Xunit.Abstractions;

namespace Dgt.Policies
{
    
    public class AsyncCacheWithCircuitBreakerPolicyTests
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Must be public to be proxied.")]
        public interface IPersonRepository
        {
            Task<Person> GetPersonAsync(Guid id);
        }

        public record Person(Guid Id, string? FirstName, string? LastName);

        private static readonly Random Random = new ();
        private static readonly string[] FirstNames = {"James", "Mark", "Luke", "John", "Homer", "Bart", "Peter", "Chris", "Bob", "Gene", "Stan", "Steve"};
        private static readonly string[] LastNames = {"Butcher", "Green", "Turner", "Robinson", "Simpson", "Griffin", "Belcher", "Smith"};

        private readonly ITestOutputHelper _output;
        private readonly Mock<IDistributedCache> _distributedCacheMock = new();
        private readonly Mock<ITtlStrategy<Person>> _ttlStrategyMock = new();
        private readonly Mock<ICacheKeyStrategy> _cacheKeyStrategyMock = new();

        private readonly AsyncCacheWithCircuitBreakerPolicy<Person> _sut;

        public AsyncCacheWithCircuitBreakerPolicyTests(ITestOutputHelper output)
        {
            _output = output;

            _distributedCacheMock
                .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]) null!);
            _distributedCacheMock.Setup(cache => cache.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback((string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token) =>
                {
                    _distributedCacheMock.Setup(cache => cache.GetAsync(key, token)).ReturnsAsync(value);
                });

            _ttlStrategyMock
                .Setup(strategy => strategy.GetTtl(It.IsAny<Context>(), It.IsAny<Person>()))
                .Returns(new Ttl(TimeSpan.MaxValue));

            _cacheKeyStrategyMock
                .Setup(strategy => strategy.GetCacheKey(It.IsAny<Context>()))
                .Returns((Context context) => context["id"]?.ToString()!);

            var serializerSettings = new JsonSerializerSettings();
            var serializer = new JsonSerializer<Person>(serializerSettings);

            // TODO This will have to be replaced with creation via the fluent API
            _sut = new AsyncCacheWithCircuitBreakerPolicy<Person>(
                _distributedCacheMock.Object.AsAsyncCacheProvider<string>().WithSerializer(serializer),
                _ttlStrategyMock.Object,
                _cacheKeyStrategyMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Should_ReturnCachedValue_When_ValueIsCached()
        {
            // Arrange
            var people = new Dictionary<Guid, Person>();
            var repoMock = new Mock<IPersonRepository>();

            // Configure the mock such that the first time we call it with a random ID we generate a random person,
            // but if we call it again with the same ID we get the same person
            repoMock
                .Setup(repo => repo.GetPersonAsync(It.IsAny<Guid>()))
                .Callback(async (Guid id) =>
                {
                    people[id] = !people.ContainsKey(id)
                        ? await CreateRandomPersonAsync(id)
                        : people[id];
                })
                .Returns((Guid id) => Task.FromResult(people[id]));

            // Act
            var personId = Guid.NewGuid();
            var context = new Context {{"id", personId}};

            var personFromRepo = await _sut.ExecuteAsync(c => repoMock.Object.GetPersonAsync(personId), context);
            var personFromCache = await _sut.ExecuteAsync(c => repoMock.Object.GetPersonAsync(personId), context);

            // Assert
            repoMock.Verify(repo => repo.GetPersonAsync(It.IsAny<Guid>()), Times.Once);
            personFromCache.Should().Be(personFromRepo);
        }

        // Generate random people so that if we accidentally create a person rather than e.g. getting one from a
        // cache we are unlikely to get a false positive
        private Task<Person> CreateRandomPersonAsync(Guid id)
        {
            _output.WriteLine("Creating person {0}", id);
            
            var firstNamesIndex = Random.Next(0, FirstNames.Length - 1);
            var lastNamesIndex = Random.Next(0, LastNames.Length - 1);

            var person = new Person(id, FirstNames[firstNamesIndex], LastNames[lastNamesIndex]);

            return Task.FromResult(person);
        }
    }
}