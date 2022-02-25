using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit;

public class NamespaceUtilityTests
{

    [Theory]
    [InlineData("/a/b/c/d", "/a/b/c", true)]
    [InlineData("/a/b/cc/d", "/a/b/c", false)]
    [InlineData("/a/b/c", "/", true)]
    public void IsAscendantOrSelf(string test, string against, bool expectedResult)
    {
        NamespaceUtility.IsSelfOrAscendant(test, against).Should().Be(expectedResult);
    }
}