using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Moq;
using Newtonsoft.Json;
using Polly;
using Polly.Caching;
using Polly.Caching.Distributed;
using Polly.Caching.Serialization.Json;
using Xunit;
using Xunit.Abstractions;

namespace CrmMicroservice.Infrastructure.Caching
{
    public class Playground
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Must be public to be proxied.")]
        public interface IPersonGenerator
        {
            Task<Person> CreatePersonAsync(Guid id);
        }

        public record Person(Guid Id, string? FirstName, string? LastName);

        private class TypeAndIdCacheKeyStrategy<T> : ICacheKeyStrategy
        {
            public string GetCacheKey(Context context)
            {
                return $"{typeof(T).Name}:{context["id"]}".ToLowerInvariant();
            }
        }

        private static readonly Random Random = new ();
        private static readonly string[] FirstNames = {"James", "Mark", "Luke", "John"};
        private static readonly string[] LastNames = {"Smith", "Green", "Turner", "Robinson"};
        
        private readonly ITestOutputHelper _output;

        public Playground(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DemonstrationOfGettingRedisCacheWithJsonSerializationAsCacheProvider()
        {
            var cacheOptions = new RedisCacheOptions
            {
                Configuration = "localhost:6379",
                InstanceName = "tests:"
            };
            var cache = new RedisCache(cacheOptions);

            var serializerSettings = new JsonSerializerSettings();
            var serializer = new JsonSerializer<Person>(serializerSettings);
            var cacheProvider = cache.AsAsyncCacheProvider<string>().WithSerializer(serializer);
            
            // Remainder of code to configure the AsyncCachePolicy

            // Silly assertion just so the compiler doesn't grumble about an unused variable
            cacheProvider.Should().NotBeNull();
        }

        [Fact]
        public async Task TestMockDistributedCacheConfiguration()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var expected = new byte[] {1, 2, 3};
            var mock = new Mock<IDistributedCache>();

            mock.Setup(x => x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback((string k, byte[] b, DistributedCacheEntryOptions o, CancellationToken t) =>
                {
                    mock.Setup(x => x.GetAsync(k, t)).ReturnsAsync(b);
                });

            // Act
            await mock.Object.SetAsync(key, expected, CancellationToken.None);

            var actual = await mock.Object.GetAsync(key, CancellationToken.None);
            var other = await mock.Object.GetAsync(Guid.NewGuid().ToString(), CancellationToken.None);

            // Assert
            actual.Should().BeEquivalentTo(expected);
            other.Should().BeEquivalentTo(Array.Empty<byte>());
        }
        
        [Fact]
        public async Task TestMockPersonGeneratorConfiguration()
        {
            // Arrange
            var id = Guid.NewGuid();
            var generator = new Mock<IPersonGenerator>();

            generator
                .Setup(gen => gen.CreatePersonAsync(It.IsAny<Guid>()))
                .Returns((Guid guid) => CreateRandomPerson(guid));

            // Act
            var person = await generator.Object.CreatePersonAsync(id);

            // Assert
            person.Id.Should().Be(id);
            
            _output.WriteLine(person.ToString());
        }

        [Fact]
        public async Task TestAsyncCachePolicyWithMockCache()
        {
            // Arrange
            // Stuff we need to build the AsyncCachePolicy
            var cacheMock = new Mock<IDistributedCache>();
            var serializerSettings = new JsonSerializerSettings();
            var serializer = new JsonSerializer<Person>(serializerSettings);
            var cacheProvider = cacheMock.Object.AsAsyncCacheProvider<string>().WithSerializer(serializer);
            var ttl = TimeSpan.MaxValue;
            var keyStrategy = new TypeAndIdCacheKeyStrategy<Person>();

            // Stuff we need to execute through the AsyncCachePolicy
            var personGeneratorMock = new Mock<IPersonGenerator>();
            var id = Guid.NewGuid();
            var context = new Context {{"id", id}};

            // And an AsyncCache policy to execute stuff through
            var sut = Policy.CacheAsync(cacheProvider, ttl, keyStrategy);

            // The documentation says the IDistributedCache interface returns null when they key does not exist in the
            // cache. By default we will actually get an empty byte array which then makes it appear as if something
            // exists for that key. When it gets deserialized it would deserialize into a null! We want the default
            // behaviour of our mock to be as if nothing is in the cache for any key
            // See https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache.getasync
            cacheMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null!);

            // This looks a bit awkward but we're configuring the mock such that if someone calls SetAsync with a given string (key)
            // and byte array (value) we will the return that byte array (value) should they call GetAsync with the same string (key).
            // In essence, we've configured the mock to actually be a cache!
            cacheMock.Setup(x => x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback((string k, byte[] b, DistributedCacheEntryOptions o, CancellationToken t) =>
                {
                    cacheMock.Setup(x => x.GetAsync(k, t)).ReturnsAsync(b);
                });

            personGeneratorMock
                .Setup(gen => gen.CreatePersonAsync(It.IsAny<Guid>()))
                .Returns((Guid guid) => CreateRandomPerson(guid));

            // Act
            Task<Person> CreatePersonAsync(Context c) =>
                personGeneratorMock.Object.CreatePersonAsync(Guid.Parse(c["id"]?.ToString()!));

            var createdPerson = await sut.ExecuteAsync(CreatePersonAsync, context);
            var cachedPerson = await sut.ExecuteAsync(CreatePersonAsync, context);

            // Assert
            // Two calls to the cache, but only one to the person generator. Two people returned, and both have the
            // same id and name data (usefulness of records for comparisons!). It seems reasonable to assume the first call
            // resulted in the person being put into the cache, and the second call returned the person from the cache
            // rather than creating another person
            cacheMock.Verify(cache => cache.GetAsync($"person:{id}", It.IsAny<CancellationToken>()), Times.Exactly(2));
            personGeneratorMock.Verify(generator => generator.CreatePersonAsync(It.IsAny<Guid>()), Times.Once);

            createdPerson.Id.Should().Be(id);
            cachedPerson.Id.Should().Be(id);
            cachedPerson.Should().Be(createdPerson);
        }

        private async Task<Person> CreateRandomPerson(Guid id)
        {
            await Task.Delay(1);

            var firstNamesIndex = Random.Next(0, FirstNames.Length - 1);
            var lastNamesIndex = Random.Next(0, LastNames.Length - 1);

            var person = new Person(id, FirstNames[firstNamesIndex], LastNames[lastNamesIndex]);

            _output.WriteLine("Created random person {0}", person);

            return person;
        }
    }
}