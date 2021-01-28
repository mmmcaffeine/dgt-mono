using System;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactCommand : IRequest<Response<CreateContactResponse>>
    {
        public string? Title { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Guid BranchId { get; init; }
    }
}