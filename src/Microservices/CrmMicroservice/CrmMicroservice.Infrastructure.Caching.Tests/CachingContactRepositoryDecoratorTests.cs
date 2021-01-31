using System;
using System.Threading;
using System.Threading.Tasks;
using Dgt.Caching;
using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace CrmMicroservice.Infrastructure.Caching
{
    public class CachingContactRepositoryDecoratorTests
    {
        private readonly Mock<IContactRepository> _contactRepositoryMock = new();
        private readonly Mock<ITypedCache> _cacheMock = new();
        private readonly CircuitBreakerOptions _options = new() {Attempts = 3, Duration = TimeSpan.FromMilliseconds(100)};
        private readonly IContactRepository _sut;

        public CachingContactRepositoryDecoratorTests()
        {
            _sut = new CachingContactRepositoryDecorator(_contactRepositoryMock.Object, _cacheMock.Object, _options);
        }

        [Fact]
        public void Ctor_Should_Throw_When_ContactRepositoryIsNull()
        {
            // Arrange
            Action action = () => _ = new CachingContactRepositoryDecorator(null!, _cacheMock.Object, _options);
            
            // Act, Assert
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("contactRepository");
        }

        [Fact]
        public void Ctor_Should_Throw_When_CacheIsNull()
        {
            // Arrange
            Action action = () => _ = new CachingContactRepositoryDecorator(_contactRepositoryMock.Object, null!, _options);
            
            // Act, Assert
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("cache");
        }

        [Fact]
        public void Ctor_Should_Throw_When_OptionsIsNull()
        {
            // Arrange
            Action action = () => _ = new CachingContactRepositoryDecorator(
                _contactRepositoryMock.Object,
                _cacheMock.Object,
                (CircuitBreakerOptions) null!);

            // Act, Assert
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("options");
        }
        
        [Fact]
        public void Ctor_Should_Throw_When_OptionsSnapshotIsNull()
        {
            // Arrange
            Action action = () => _ = new CachingContactRepositoryDecorator(
                _contactRepositoryMock.Object,
                _cacheMock.Object,
                (IOptionsSnapshot<CircuitBreakerOptions>) null!);

            // Act, Assert
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("optionsSnapshot");
        } 

        [Fact]
        public async Task GetContactAsync_Should_GetContactFromRepository_When_ContactIsNotCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new ContactEntity {Id = id};

            _cacheMock
                .Setup(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()))
                .ReturnsAsync((ContactEntity?) null);
            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetContactAsync(id);

            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetContactAsync_Should_CacheContactFromRepository_When_ContactIsNotCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var expected = new ContactEntity {Id = id};

            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(key, expected, It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task GetContactAsync_Should_NotThrow_When_CachingContactFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contact);
            _cacheMock
                .Setup(cache => cache.SetRecordAsync(key, contact, It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new Exception("Kaboom!"));

            // Act, Assert
            await _sut.Invoking(repo => repo.GetContactAsync(id)).Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetContactAsync_Should_GetContactFromCache_When_ContactIsCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var expected = new ContactEntity {Id = id};

            _cacheMock.Setup(cache => cache.GetRecordAsync<ContactEntity>(key)).ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetContactAsync(id);

            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetContactAsync_Should_FallBackToRepository_When_CacheIsInError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new ContactEntity {Id = id};

            _cacheMock
                .Setup(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Kaboom!"));
            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetContactAsync(id);
            
            // Assert
            actual.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task GetContactAsync_Should_BreakCircuitToCache_When_CacheIsInError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();

            _cacheMock
                .Setup(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Kaboom!"));

            // Act
            for (var i = 0; i < _options.Attempts; i++)
            {
                _ = await _sut.GetContactAsync(id);
            }

            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.GetRecordAsync<ContactEntity>(key), Times.Exactly(_options.Attempts));
        }

        [Fact]
        public async Task GetContactAsync_Should_ResetBrokenCircuitToCache_When_TimeHasPassed()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            var sequence = _cacheMock.SetupSequence(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()));
            for (var i = 0; i < _options.Attempts; i++)
            {
                sequence = sequence.ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Kaboom!"));
            }
            sequence.ReturnsAsync(contact);

            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contact);

            // Act
            for (var i = 0; i < _options.Attempts; i++)
            {
                _ = await _sut.GetContactAsync(id);
            }

            await Task.Delay(_options.Duration.Add(TimeSpan.FromMilliseconds(10)));
            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.GetRecordAsync<ContactEntity>(key), Times.Exactly(_options.Attempts + 1));
            _contactRepositoryMock.Verify(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()), Times.Exactly(_options.Attempts));
        }

        [Fact]
        public async Task GetContactAsync_Should_NotAttemptToCacheContact_When_CacheIsInError()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contact = new ContactEntity {Id = id};

            _cacheMock
                .Setup(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()))
                .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Kaboom!"));
            _contactRepositoryMock
                .Setup(repo => repo.GetContactAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(contact);

            // Act
            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContactEntity>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()),
                Times.Never);
        }

        [Fact]
        public async Task GetContactAsync_Should_NotGetContactFromRepository_When_ContactIsCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            _cacheMock.Setup(cache => cache.GetRecordAsync<ContactEntity>(key)).ReturnsAsync(contact);

            // Act
            _ = await _sut.GetContactAsync(id);

            // Assert
            _contactRepositoryMock.Verify(repo => repo.GetContactAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetContactAsync_Should_NotRecacheContact_When_ContactIsCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            _cacheMock.Setup(cache => cache.GetRecordAsync<ContactEntity>(key)).ReturnsAsync(contact);

            // Act
            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContactEntity>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()),
                Times.Never);
        }

        [Fact]
        public async Task InsertContactAsync_Should_InsertContact_When_ContactInserted()
        {
            // Arrange
            using var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var id = Guid.NewGuid();
            var contact = new ContactEntity {Id = id};

            // Act
            await _sut.InsertContactAsync(contact, token);

            // Assert
            _contactRepositoryMock.Verify(repo => repo.InsertContactAsync(contact, token), Times.Once);
        }

        [Fact]
        public async Task InsertContactAsync_Should_CacheContact_When_ContactIsInserted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            // Act
            await _sut.InsertContactAsync(contact);

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(
                    key,
                    contact,
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()),
                Times.Once);
        }

        [Fact]
        public async Task InsertContactAsync_Should_NotCacheContact_When_InsertingContactFails()
        {
            // Arrange
            var contact = new ContactEntity {Id = Guid.NewGuid()};

            _contactRepositoryMock
                .Setup(repo => repo.InsertContactAsync(It.IsAny<ContactEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Kaboom!"));

            // Act
            try
            {
                // ENHANCE You need your InvokeSilently extension method here!
                await _sut.InsertContactAsync(contact);
            }
            catch
            {
                // Deliberately suppress exceptions so we can verify caching
            }

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(
                    It.IsAny<string>(),
                    It.IsAny<ContactEntity>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<TimeSpan?>()),
                Times.Never);
        }

        [Fact]
        public async Task InsertContactAsync_Should_NotThrow_When_CachingContactFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var key = $"{nameof(ContactEntity)}:{id}".ToLowerInvariant();
            var contact = new ContactEntity {Id = id};

            _cacheMock
                .Setup(cache => cache.SetRecordAsync(key, contact, It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>()))
                .ThrowsAsync(new Exception("Kaboom!"));

            // Act, Assert
            await _sut.Invoking(repo => repo.InsertContactAsync(contact)).Should().NotThrowAsync();
        }
    }
}