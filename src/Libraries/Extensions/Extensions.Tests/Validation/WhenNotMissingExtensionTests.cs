using System;
using FluentAssertions;
using Xunit;

namespace Dgt.Extensions.Validation
{
    public static class WhenNotMissingExtensionTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
        public static void WhenNotMissing_Given_ValueIsMissingAndParamNameIsNull_Then_ExceptionThrown(string value)
        {
            value.Invoking(s => s.WhenNotMissing())
                .Should().Throw<ArgumentException>()
                .WithMessage("Value cannot be null, whitespace, or an empty string.*")
                .And.ParamName.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r\n")]
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