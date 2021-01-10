using Dgt.Options;
using FluentValidation;

namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    public class FileBasedRepositoryOptions : OptionsValidatorBase<FileBasedRepositoryOptions>
    {
        public const string Repositories = "Repositories";
        public const string ContactRepository = "Contact";
        public const string BranchRepository = "Branch";
        
        public FileBasedRepositoryOptions()
        {
            RuleFor(x => x.Path).NotEmpty();
            RuleFor(x => x.Delay).GreaterThanOrEqualTo(0);
        }

        public string Path { get; set; } = default!;
        public int Delay { get; set; }

        public void Deconstruct(out string path, out int delay)
        {
            path = Path;
            delay = Delay;
        }
    }
}