namespace Dgt.CrmMicroservice.Infrastructure.FileBased
{
    // TODO Validate paths are not null
    // TODO Validate delay is not negative
    public class FileBasedRepositoryOptions
    {
        public string? ContactsPath { get; set; }
        public string? BranchesPath { get; set; }
        public int Delay { get; set; }
    }
}