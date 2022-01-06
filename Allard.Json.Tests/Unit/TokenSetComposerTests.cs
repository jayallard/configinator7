using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Allard.Json.Tests.Unit;

public class TokenSetComposerTests
{
    [Fact]
    public void Compose()
    {
        var c = new TokenSet
        {
            TokenSetName = "c",
            Tokens = new Dictionary<string, JToken>
            {
                {"c1", "c1"},
                {"c2", 22},
                {"c3", JToken.Parse("{ \"hello\": \"galaxy\" }")},
            }
        };
        var b = new TokenSet
        {
            Base = "c",
            TokenSetName = "b",
            Tokens = new Dictionary<string, JToken>
            {
                {"c1", "b1"},
                {"c3", JToken.Parse("{ \"hello\": \"sun\" }")},
                {"c4", JToken.Parse("{ \"new\": \"guy\" }")},
            }
        };
        var a = new TokenSet
        {
            Base = "b",
            TokenSetName = "a",
            Tokens = new Dictionary<string, JToken>
            {
                {"c2", 99},
                {"c5", "smashing"},
            }
        };

        var resolver = new TokenSetComposer(new[] {a, b, c});
        var aResolved = resolver.Compose("a");
        aResolved.Tokens.Count.Should().Be(5);

        // c1 is defined in c, overridden in b, inherited by a
        aResolved.Tokens["c1"].TokenValueOrigin.Should().Be(TokenValueOrigin.Inherited);
        aResolved.Tokens["c1"].SourceTokenSet.Should().Be("b");
        aResolved.Tokens["c1"].Token?.Value<string>().Should().Be("b1");

        // c2 is defined in c, overridden in a
        aResolved.Tokens["c2"].TokenValueOrigin.Should().Be(TokenValueOrigin.Override);
        aResolved.Tokens["c2"].SourceTokenSet.Should().Be("a");
        aResolved.Tokens["c2"].Token?.Value<int>().Should().Be(99);

        // c3 is defined in c, overridden in b, inherited by a
        aResolved.Tokens["c3"].TokenValueOrigin.Should().Be(TokenValueOrigin.Inherited);
        aResolved.Tokens["c3"].SourceTokenSet.Should().Be("b");
        JToken.DeepEquals(aResolved.Tokens["c3"].Token, JToken.Parse("{ \"hello\": \"sun\" }")).Should().BeTrue();

        // c4 is defined in b, not overridden, inherited by a
        aResolved.Tokens["c4"].TokenValueOrigin.Should().Be(TokenValueOrigin.Inherited);
        aResolved.Tokens["c4"].SourceTokenSet.Should().Be("b");
        JToken.DeepEquals(aResolved.Tokens["c4"].Token, JToken.Parse("{ \"new\": \"guy\" }")).Should().BeTrue();

        // c5 is defined in a, not overridden
        aResolved.Tokens["c5"].TokenValueOrigin.Should().Be(TokenValueOrigin.Addition);
        aResolved.Tokens["c5"].SourceTokenSet.Should().Be("a");
        aResolved.Tokens["c5"].Token?.ToString().Should().Be("smashing");
    }
}