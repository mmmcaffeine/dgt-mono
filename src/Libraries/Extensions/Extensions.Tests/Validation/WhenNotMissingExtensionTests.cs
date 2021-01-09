using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Dgt.Extensions.Validation
{
    public static class WhenNotMissingExtensionTests
    {
        public static IEnumerable<object?[]> MissingStrings
        {
            get
            {
                yield return new object?[] {(string?) null};
                yield return new object?[] {string.Empty};
                yield return new object?[] {"  "};
                yield return new object?[] {"\t"};
                yield return new object?[] {Environment.NewLine};
            }
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public static void WhenNotMissing_Given_ValueIsMissingAndParamNameIsNull_Then_ExceptionThrown(string value)
        {
            value.Invoking(s => s.WhenNotMissing())
                .Should().Throw<ArgumentException>()
                .WithMessage("Value cannot be null, whitespace, or an empty string.*")
                .And.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(MissingStrings))]
        public static void WhenNotMissing_Given_ValueIsMissingAndParamNameIsNotNull_Then_ExceptionThrown(string value)
        {
            value.Invoking(s => s.WhenNotMissing("parameter"))
                .Should().Throw<ArgumentException>()
                .WithMessage("Value cannot be null, whitespace, or an empty string.*")
                .And.ParamName.Should().Be("parameter");
        }

        [Fact]
        public static void WhenNotMissing_Given_ValueIsNotMissing_Then_Value()
        {
            // Arrange
            const string expected = "This is the expected return value";

            // Act
            var actual = expected.WhenNotMissing();

            // Assert
            actual.Should().Be(expected);
        }
    }
}