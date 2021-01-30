using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Options;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedContactRepository : IContactRepository
    {
        private class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
        {
            private const string SplitWordsPattern = "((?<=[a-z])[A-Z]|(?<!^)[A-Z](?=[a-z]))";
            
            public override string ConvertName(string name)
            {
                return string.IsNullOrWhiteSpace(name)
                    ? name
                    : Regex.Replace(name, SplitWordsPattern, x => $"_{x.Value}").ToLowerInvariant();
            }
        }
        
        private record ContactDto(Guid Id, string? Title, string? FirstName, string? LastName, Guid BranchId)
        {
            [return: NotNullIfNotNull("dto")]
            public static explicit operator ContactEntity?(ContactDto? dto)
            {
                if (dto is null)
                {
                    return null;
                }

                return new ContactEntity
                {
                    Id = dto.Id,
                    Title = dto.Title,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    BranchId = dto.BranchId
                };
            }

            [return: NotNullIfNotNull("entity")]
            public static explicit operator ContactDto?(ContactEntity? entity)
            {
                if (entity is null)
                {
                    return null;
                }

                return new ContactDto
                (
                    entity.Id,
                    entity.Title,
                    entity.FirstName,
                    entity.LastName,
                    entity.BranchId
                );
            }
        }

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy()
        };

        private readonly string _path;
        private readonly int _delay;

        public FileBasedContactRepository(IOptionsSnapshot<FileBasedRepositoryOptions> optionsSnapshot)
        {
            var options = optionsSnapshot
                .WhenNotNull(nameof(optionsSnapshot))
                .Get(FileBasedRepositoryOptions.ContactRepository);

            (_path, _delay) = options;
        }

        public async Task<IQueryable<ContactEntity>> GetContactsAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);

            var json = await File.ReadAllTextAsync(_path, cancellationToken);
            var dtos = JsonSerializer.Deserialize<List<ContactDto>>(json, JsonSerializerOptions)!;

            return dtos.Select(dto => (ContactEntity) dto).AsQueryable();
        }

        public async Task<ContactEntity?> GetContactAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);

            var json = await File.ReadAllTextAsync(_path, cancellationToken);
            var dtos = JsonSerializer.Deserialize<List<ContactDto>>(json, JsonSerializerOptions)!;
            var dto = dtos.FirstOrDefault(x => x.Id == id);

            return dto is not null ? (ContactEntity) dto : null;
        }

        public async Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);
            await using var stream = File.Open(_path, FileMode.Open, FileAccess.ReadWrite);

            var dtos = (await JsonSerializer.DeserializeAsync<List<ContactDto>>(stream, JsonSerializerOptions, cancellationToken))!;

            dtos.Add((ContactDto) contact);
            stream.SetLength(0);

            await JsonSerializer.SerializeAsync(stream, dtos, JsonSerializerOptions, cancellationToken);
        }
    }
}