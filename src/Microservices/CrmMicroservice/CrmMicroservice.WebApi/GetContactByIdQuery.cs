using System;
using MediatR;

namespace Dgt.CrmMicroservice.WebApi
{
    public class GetContactByIdQuery : IRequest<GetContactByIdResponse>
    {
        public Guid Id { get; init; }

        public static implicit operator GetContactByIdQuery(Guid id) => new GetContactByIdQuery {Id = id};
        public static implicit operator Guid(GetContactByIdQuery? query) => query?.Id ?? Guid.Empty;
    }
}