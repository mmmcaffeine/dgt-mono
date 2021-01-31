using System;

namespace Dgt.CrmMicroservice.Infrastructure.Caching
{
    // TODO Validation. Attempts has to be positive, non-zero
    public class CircuitBreakerOptions
    {
        public int Attempts { get; set; }
        public TimeSpan Duration { get; set; }
    }
}