using System;

namespace Dgt.CrmMicroservice.WebApi
{
    public class GetContactByIdResponse
    {
        public Guid Id { get; init; }
        public string? Title { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Guid BranchId { get; init; }
    }
}