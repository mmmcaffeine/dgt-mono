using System;
using System.Collections.Generic;

namespace Dgt.CrmMicroservice.Domain
{
    public class BranchEntity
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public IEnumerable<Guid> ContactIds { get; init; } = Array.Empty<Guid>();
    }
}