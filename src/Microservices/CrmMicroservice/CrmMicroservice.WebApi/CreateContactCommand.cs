using System;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactCommand : IRequest<CreateContactResponse>
    {
        public string? Title { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Guid BranchId { get; init; }
    }
}