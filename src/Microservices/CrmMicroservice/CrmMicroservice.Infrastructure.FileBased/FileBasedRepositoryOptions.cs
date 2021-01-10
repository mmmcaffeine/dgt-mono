using Dgt.Options;
using FluentValidation;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedRepositoryOptions : OptionsValidatorBase<FileBasedRepositoryOptions>
    {
        public FileBasedRepositoryOptions()
        {
            RuleFor(x => x.ContactsPath).NotEmpty();
            RuleFor(x => x.BranchesPath).NotEmpty();
            RuleFor(x => x.Delay).GreaterThanOrEqualTo(0);
        }

        public string ContactsPath { get; set; } = default!;

        public string BranchesPath { get; set; } = default!;

        public int Delay { get; set; }
    }
}