using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dgt.CrmMicroservice.Domain;
using Dgt.Extensions.Validation;
using Microsoft.Extensions.Options;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedBranchRepository : IBranchRepository
    {
        private record BranchDto(Guid Id, string Name, IEnumerable<Guid> Contacts);

        private readonly string _path;
        private readonly int _delay;

        public FileBasedBranchRepository(IOptions<FileBasedRepositoryOptions> options)
        {
            var value = options.Value.WhenNotNull(nameof(options));
            
            _path = value.BranchesPath;
            _delay = value.Delay;
        }

        public async Task<BranchEntity> GetBranchAsync(Guid id)
        {
            await Task.Delay(_delay);

            var json = await File.ReadAllTextAsync(_path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var dtos = JsonSerializer.Deserialize<List<BranchDto>>(json, options);
            // TODO Better exception handling e.g. multiple matches
            var (_, name, contactIds) = dtos?.SingleOrDefault(x => x.Id == id)
                                        ?? throw new ArgumentException("No entity with the supplied ID exists.", nameof(id));

            return new BranchEntity
            {
                Id = id,
                Name = name,
                ContactIds = contactIds
            };
        }
    }
}