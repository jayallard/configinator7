using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit;

public class NamespaceUtilityTests
{
    [Theory]
    [InlineData("/a/b/c/d", "/a/b/c", true)]
    [InlineData("/a/b/cc/d", "/a/b/c", false)]
    [InlineData("/a/b/c", "/", true)]
    public void IsAscendantOrSelf(string descendant, string ascendant, bool expectedResult)
    {
        NamespaceUtility.IsSelfOrAscendant(ascendant, descendant).Should().Be(expectedResult);
    }
}