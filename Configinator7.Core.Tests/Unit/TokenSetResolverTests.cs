using System.Collections.Generic;
using Configinator7.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Configinator7.Core.Tests.Unit;

public class TokenSetResolverTests
{
    [Fact]
    public void Blah()
    {
        var c = new TokenSet
        {
            TokenSetName = "c",
            Tokens = new Dictionary<string, JToken>
            {
                {"c1", JToken.Parse("\"c1\"")},
                {"c2", JToken.Parse("22")},
                {"c3", JToken.Parse("{ \"hello\": \"galaxy\" }")},
            }
        };
        var b = new TokenSet
        {
            Base = "c",
            TokenSetName = "b",
            Tokens = new Dictionary<string, JToken>
            {
                {"c1", JToken.Parse("\"b1\"")},
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
                {"c2", JToken.Parse("99")},
                {"c5", "\"smashing\""},
            }
        };

        var resolver = new TokenSetResolver(new[] {a, b, c});
        var aResolved = resolver.Resolve("a");
        aResolved.Tokens.Count.Should().Be(5);

        // c1 is defined in c, overridden in b, inherited by a
        aResolved.Tokens["c1"].Resolution.Should().Be(Resolution.Inherited);
        aResolved.Tokens["c1"].SourceTokenSet.Should().Be("b");
        aResolved.Tokens["c1"].Value.ToString().Should().Be("b1");
        
        // c2 is defined in c, overridden in a
        aResolved.Tokens["c2"].Resolution.Should().Be(Resolution.Override);
        aResolved.Tokens["c2"].SourceTokenSet.Should().Be("a");
        aResolved.Tokens["c2"].Value.ToString().Should().Be("99");
        
        // c3 is defined in c, overridden in b, inherited by a
        aResolved.Tokens["c3"].Resolution.Should().Be(Resolution.Inherited);
        aResolved.Tokens["c3"].SourceTokenSet.Should().Be("b");
        JToken.DeepEquals(aResolved.Tokens["c3"].Value, JToken.Parse("{ \"hello\": \"sun\" }")).Should().BeTrue();
        
        // c4 is defined in b, not overridden, inherited by a
        aResolved.Tokens["c4"].Resolution.Should().Be(Resolution.Inherited);
        aResolved.Tokens["c4"].SourceTokenSet.Should().Be("b");
        JToken.DeepEquals(aResolved.Tokens["c4"].Value, JToken.Parse("{ \"new\": \"guy\" }")).Should().BeTrue();
        
        // c5 is defined in a, not overridden
        aResolved.Tokens["c5"].Resolution.Should().Be(Resolution.Addition);
        aResolved.Tokens["c5"].SourceTokenSet.Should().Be("a");
        aResolved.Tokens["c5"].Value.ToString().Should().Be("\"smashing\"");
    }
}