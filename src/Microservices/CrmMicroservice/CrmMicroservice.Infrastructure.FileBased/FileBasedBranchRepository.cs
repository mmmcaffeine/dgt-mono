using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Options;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedBranchRepository : IBranchRepository
    {
        private record BranchDto(Guid Id, string Name, IEnumerable<Guid> Contacts)
        {
            [return: NotNullIfNotNull("dto")]
            public static explicit operator BranchEntity?(BranchDto? dto)
            {
                if (dto is null) return null;
                
                return new BranchEntity
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    ContactIds = dto.Contacts
                };
            }
        }

        private readonly string _path;
        private readonly int _delay;

        public FileBasedBranchRepository(IOptionsSnapshot<FileBasedRepositoryOptions> optionsSnapshot)
        {
            var options = optionsSnapshot
                .WhenNotNull(nameof(optionsSnapshot))
                .Get(FileBasedRepositoryOptions.BranchRepository);

            (_path, _delay) = options;
        }

        public async Task<BranchEntity> GetBranchAsync(Guid id)
        {
            var entities = await GetBranchesAsync();

            // TODO Better exception handling e.g. multiple matches
            return entities.SingleOrDefault(x => x.Id == id)
                ?? throw new ArgumentException("No entity with the supplied ID exists.", nameof(id));
        }

        public async Task<IQueryable<BranchEntity>> GetBranchesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);
            
            var json = await File.ReadAllTextAsync(_path, cancellationToken);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var dtos = JsonSerializer.Deserialize<List<BranchDto>>(json, options)!;

            return dtos.Select(dto => (BranchEntity) dto).AsQueryable();
        }
    }
}