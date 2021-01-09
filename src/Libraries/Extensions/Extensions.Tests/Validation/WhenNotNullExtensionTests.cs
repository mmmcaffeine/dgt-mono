using System;
using FluentAssertions;
using Xunit;

namespace Dgt.Extensions.Validation
{
    public class WhenNotNullExtensionTests
    {
        [Fact]
        public static void WhenNotNull_GivenValueIsNullAndParamNameIsNull_Then_ExceptionThrown()
        {
            ((object?) null).Invoking(x => x.WhenNotNull())
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null.*")
                .And.ParamName.Should().BeNull();
        }

        [Fact]
        public static void WhenNotNull_GivenValueIsNullAndParamNameIsNotNull_Then_ExceptionThrown()
        {
            ((object?) null).Invoking(x => x.WhenNotNull("parameter"))
                .Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null.*")
                .And.ParamName.Should().Be("parameter");
        }

        [Fact]
        public static void WhenNotNull_Given_ValueIsNotNull_Then_Value()
        {
            // Arrange
            var expected = new { };

            // Act
            var actual = expected.WhenNotNull();

            // Assert
            actual.Should().BeSameAs(expected);
        }
    }
}