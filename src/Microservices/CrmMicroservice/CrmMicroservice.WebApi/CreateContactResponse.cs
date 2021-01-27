using System;

namespace Dgt.CrmMicroservice.WebApi
{
    public class CreateContactResponse
    {
        public Guid Id { get; init; }
        public string Uri { get; init; } = default!;
    }
}