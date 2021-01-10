using System.ComponentModel.DataAnnotations;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedRepositoryOptions
    {
        [Required]
        public string ContactsPath { get; set; } = default!;

        [Required]
        public string BranchesPath { get; set; } = default!;

        [Range(0, int.MaxValue)]
        public int Delay { get; set; }
    }
}