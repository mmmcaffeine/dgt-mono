using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Dgt.Caching
{
    // REM Ideally we would check we've call the SetStringAsync method correctly. However, that is also an extension
    //     so we can't assert against it. The best we can do is to check the method that extension calls!

    // REM It looks very peculiar that we instantiated a mock, and then used that as our SUT. This is deliberate. It
    //     allows us to verify we have eventually resolved to the correct overload on the interface, and passed through
    //     the appropriate parameters
    public class DistributedCacheExtensionsTests
    {
        private record Person(string? FirstName, string? LastName);

        private readonly Mock<IDistributedCache> _cacheMock = new();
        private readonly IDistributedCache _sut;

        public DistributedCacheExtensionsTests()
        {
            _sut = _cacheMock.Object;
        }

        public static TheoryData<string?> MissingStringTheoryData => new()
        {
            null,
            string.Empty,
            "  ",
            "\t",
            Environment.NewLine
        };

        [Theory]
        [MemberData(nameof(MissingStringTheoryData))]
        public void SetRecordAsync_Should_ThrowException_When_KeyIsMissing(string? key)
        {
            // Arrange
            var value = new Person("Homer", "Simpson");

            // Act, Assert
            _sut.Invoking(cache => cache.SetRecordAsync(key!, value))
                .Should().Throw<ArgumentException>()
                .WithMessage("Value cannot be null, whitespace, or an empty string.*")
                .And.ParamName.Should().Be("key");
        }

        [Fact]
        public void SetRecordAsync_Should_ThrowException_When_ValueIsNull()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Act, Assert
            _sut.Invoking(cache => cache.SetRecordAsync(key, (Person) null!))
                .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("value");
        }

        [Fact]
        public async Task SetRecordAsync_Should_SetSerializedBytes_When_GivenKeyAndValue()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = new Person("Homer", "Simpson");

            // Act
            await _sut.SetRecordAsync(key, value);

            // Assert
            const string json = "{\"FirstName\":\"Homer\",\"LastName\":\"Simpson\"}";
            var bytes = Encoding.UTF8.GetBytes(json);

            _cacheMock.Verify(cache => cache.SetAsync
                (key,
                bytes,
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task SetRecordAsync_Should_SetDefaultAbsoluteExpirationOfOneMinuteOnOptions_When_NoAbsoluteExpirationSupplied()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = new Person("Homer", "Simpson");

            // Act
            await _sut.SetRecordAsync(key, value);

            // Assert
            Func<DistributedCacheEntryOptions, bool> optionsMatcher = options =>
                options.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(1);

            _cacheMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options => optionsMatcher(options)),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task SetRecordAsync_Should_SetAbsoluteExpirationOnOptions_When_AbsoluteExpirationSupplied()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = new Person("Homer", "Simpson");
            var absoluteExpiration = new TimeSpan(0, 1, 30);

            // Act
            await _sut.SetRecordAsync(key, value, absoluteExpiration);

            // Assert
            Func<DistributedCacheEntryOptions, bool> optionsMatcher = options =>
                options.AbsoluteExpirationRelativeToNow == absoluteExpiration;
            _cacheMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options => optionsMatcher(options)),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task SetRecordAsync_Should_NotSetSlidingExpirationOnOptions_When_NoSlidingExpirationSupplied()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = new Person("Homer", "Simpson");

            // Act
            await _sut.SetRecordAsync(key, value);

            // Assert
            Func<DistributedCacheEntryOptions, bool> optionsMatcher = options => options.SlidingExpiration is null;
            _cacheMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options => optionsMatcher(options)),
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task SetRecordAsync_Should_SetSlidingExpirationOnOptions_When_SlidingExpirationSupplied()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var value = new Person("Homer", "Simpson");
            var slidingExpiration = new TimeSpan(0, 1, 30);

            // Act
            await _sut.SetRecordAsync(key, value, slidingExpiration: slidingExpiration);

            // Assert
            Func<DistributedCacheEntryOptions, bool> optionsMatcher = options =>
                options.SlidingExpiration == slidingExpiration;
            _cacheMock.Verify(cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(options => optionsMatcher(options)),
                It.IsAny<CancellationToken>()));
        }
    }
}