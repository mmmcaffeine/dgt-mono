using System;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class GetContactByIdQuery : IRequest<GetContactByIdResponse>
    {
        public Guid Id { get; init; }
    }
}