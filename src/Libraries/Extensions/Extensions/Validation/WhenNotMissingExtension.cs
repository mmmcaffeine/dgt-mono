using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Dgt.Extensions.Validation
{
    public static class WhenNotMissingExtension
    {
        private const string MissingStringMessage = "Value cannot be null, whitespace, or an empty string.";

        // ENHANCE Figure out how to say paramName should match a parameter name as ArgumentNullException does
        [DebuggerHidden]
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        public static string WhenNotMissing([NotNull] this string? value, string? paramName = null)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (paramName is null)
            {
                throw new ArgumentException(MissingStringMessage);
            }
            else
            {
                throw new ArgumentException(MissingStringMessage, paramName);
            }
        }
    }
}