using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Dgt.Extensions.Validation
{
    public static class WhenNotNullExtension
    {
        // ENHANCE Figure out how to say paramName should match a parameter name as ArgumentNullException does
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        public static T WhenNotNull<T>([NotNull] this T value, string? paramName = null)
        {
            if (value is not null)
            {
                return value;
            }

            if (paramName is null)
            {
                throw new ArgumentNullException();
            }
            else
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}