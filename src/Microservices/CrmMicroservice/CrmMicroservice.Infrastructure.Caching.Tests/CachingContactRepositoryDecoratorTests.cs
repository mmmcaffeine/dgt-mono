using System;
using System.Threading.Tasks;
using Dgt.Caching;
using Dgt.CrmMicroservice.Domain;
using Dgt.CrmMicroservice.Infrastructure.Caching;
using FluentAssertions;
using Moq;
using Xunit;

namespace CrmMicroservice.Infrastructure.Caching
{
    public class CachingContactRepositoryDecoratorTests
    {
        private readonly Mock<IContactRepository> _contactRepositoryMock = new();
        private readonly Mock<ITypedCache> _cacheMock = new();
        private readonly IContactRepository _sut;
        
        public CachingContactRepositoryDecoratorTests()
        {
            _sut = new CachingContactRepositoryDecorator(_contactRepositoryMock.Object, _cacheMock.Object);
        }

        [Fact]
        public async Task GetContactAsync_Should_GetContactFromRepository_When_ContactIsNotCached()
        {
            // Arrange
            var id = Guid.NewGuid();
            var expected = new ContactEntity {Id = id};

            _cacheMock.Setup(cache => cache.GetRecordAsync<ContactEntity>(It.IsAny<string>()))
                .ReturnsAsync((ContactEntity?) null);
            _contactRepositoryMock.Setup(repo => repo.GetContactAsync(id)).ReturnsAsync(expected);

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

            _contactRepositoryMock.Setup(repo => repo.GetContactAsync(id)).ReturnsAsync(expected);

            // Act
            _ = await _sut.GetContactAsync(id);

            // Assert
            _cacheMock.Verify(cache => cache.SetRecordAsync(key, expected, It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>()), Times.Once);
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
            _contactRepositoryMock.Verify(repo => repo.GetContactAsync(It.IsAny<Guid>()), Times.Never);
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
    }
}