using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedContactRepository : IContactRepository
    {
        private class ContactDto
        {
            public Guid Id { get; init; }

            public string? Title { get; init; }

            [JsonPropertyName("first_name")]
            public string? FirstName { get; init; }

            [JsonPropertyName("last_name")]
            public string? LastName { get; init; }

            [JsonPropertyName("branch_id")]
            public Guid BranchId { get; init; }

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
        }

        private readonly string _path;
        private readonly int _delay;

        // TODO Validate path is not null
        // TODO Validate delay is not negative
        public FileBasedContactRepository(string path, int delay)
        {
            _path = path;
            _delay = delay;
        }

        // For now we have the same shape as the ContactEntity so we _could_ deserialize directly into that
        public async Task<ContactEntity> GetContactAsync(Guid id)
        {
            await Task.Delay(_delay);

            var json = await File.ReadAllTextAsync(_path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var dtos = JsonSerializer.Deserialize<List<ContactDto>>(json, options);

            // TODO Exception handling
            // No matches
            // Multiple matches (FUBAR source data)
            // Nulls (I've basic mangled the NRT feature here...)
            return (ContactEntity) dtos?.First()!;
        }
    }
}