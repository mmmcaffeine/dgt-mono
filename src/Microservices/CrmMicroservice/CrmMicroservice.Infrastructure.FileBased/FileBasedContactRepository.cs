﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Options;

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
                {
                    Id = entity.Id,
                    Title = entity.Title,
                    FirstName = entity.FirstName,
                    LastName = entity.LastName,
                    BranchId = entity.BranchId
                };
            }
        }

        private readonly string _path;
        private readonly int _delay;

        public FileBasedContactRepository(IOptionsSnapshot<FileBasedRepositoryOptions> optionsSnapshot)
        {
            var options = optionsSnapshot
                .WhenNotNull(nameof(optionsSnapshot))
                .Get(FileBasedRepositoryOptions.ContactRepository);

            (_path, _delay) = options;
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

        public async Task InsertContactAsync([NotNull] ContactEntity contact, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            await using var stream = File.Open(_path, FileMode.Open, FileAccess.ReadWrite);

            var dtos = (await JsonSerializer.DeserializeAsync<List<ContactDto>>(stream, options, cancellationToken))!;

            dtos.Add((ContactDto) contact);
            stream.SetLength(0);

            await JsonSerializer.SerializeAsync(stream, dtos, options, cancellationToken);
        }
    }
}