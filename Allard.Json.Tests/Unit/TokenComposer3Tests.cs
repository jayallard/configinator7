using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Json.Tests.Unit;

public class TokenComposer3Tests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TokenComposer3Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void TokenHierarchy()
    {
        var sets = new[]
        {
            new TokenSet
            {
                TokenSetName = "a",
                Tokens =
                {
                    {"a", "from a"}
                }
            },
            new TokenSet
            {
                TokenSetName = "b",
                BaseTokenSetName = "a",
                Tokens =
                {
                    {"b", "from b"}
                }
            },
            new TokenSet
            {
                TokenSetName = "c",
                BaseTokenSetName = "b",
                Tokens =
                {
                    {"a", "from c"}
                }
            },
            new TokenSet
            {
                TokenSetName = "d",
                BaseTokenSetName = "c",
            },
        };

        var a = TokenComposer3.Compose(sets, "a");
        var b = a.GetChild("b");
        var c = b.GetChild("c");
        var d = c.GetChild("d");
        
        // it's defined in A
        var tokenAFromA = a.GetToken("a");
        tokenAFromA.Origin.Should().Be(TokenValueOrigin.Defined);

        var tokenAFromB = b.GetToken("a");
        tokenAFromB.Origin.Should().Be(TokenValueOrigin.Inherited);

        var tokenAFromC = c.GetToken("a");
        tokenAFromC.Origin.Should().Be(TokenValueOrigin.Override);

        var tokenAFromD = d.GetToken("a");
        tokenAFromD.Origin.Should().Be(TokenValueOrigin.Inherited);
    }

    [Fact]
    public void AllKeys()
    {
        var sets = new[]
        {
            new TokenSet
            {
                TokenSetName = "a",
                Tokens =
                {
                    {"a", "from a"}
                }
            },
            new TokenSet
            {
                TokenSetName = "b",
                BaseTokenSetName = "a",
                Tokens =
                {
                    {"b", "from b"}
                }
            },
            new TokenSet
            {
                TokenSetName = "c",
                BaseTokenSetName = "b",
                Tokens =
                {
                    {"a", "from c"},
                    {"c", "from c"}
                }
            },
            new TokenSet
            {
                TokenSetName = "d",
                BaseTokenSetName = "c",
                Tokens =
                {
                }
            },
            new TokenSet
            {
                TokenSetName = "e",
                BaseTokenSetName = "d",
                Tokens =
                {
                    {"e", "from e"}
                }
            },
        };

        var a = TokenComposer3.Compose(sets, "a");
        var b = a.GetChild("b");
        var c = b.GetChild("c");
        var d = c.GetChild("d");
        var e = d.GetChild("e");

        // a
        a.Keys.Count.Should().Be(1);
        
        // a, b
        b.Keys.Count.Should().Be(2);
        
        // a, b, c
        c.Keys.Count.Should().Be(3);
        
        // a, b, c
        d.Keys.Count.Should().Be(3);
        
        // a, b, c, 3
        e.Keys.Count.Should().Be(4);
    }


    [Fact]
    public void TokenSetHierarchy()
    {
        var sets = new[]
        {
            new TokenSet {TokenSetName = "a"},
            new TokenSet {TokenSetName = "b1", BaseTokenSetName = "a"},
            new TokenSet {TokenSetName = "b2", BaseTokenSetName = "a"},
            new TokenSet {TokenSetName = "c1", BaseTokenSetName = "b1"},
            new TokenSet {TokenSetName = "c2", BaseTokenSetName = "b1"},
            new TokenSet {TokenSetName = "c3", BaseTokenSetName = "b1"},
            new TokenSet {TokenSetName = "d1", BaseTokenSetName = "c3"},
            new TokenSet {TokenSetName = "d2", BaseTokenSetName = "c3"},
            new TokenSet {TokenSetName = "x", BaseTokenSetName = "b2"},
            new TokenSet {TokenSetName = "y", BaseTokenSetName = "x"},
            new TokenSet {TokenSetName = "z", BaseTokenSetName = "y"},
        };

        var a = TokenComposer3.Compose(sets, "a");
        var b1 = a.GetChild("b1");
        var b2 = a.GetChild("b2");
        var c1 = b1.GetChild("c1");
        var c2 = b1.GetChild("c2");
        var c3 = b1.GetChild("c3");
        var d1 = c3.GetChild("d1");
        var d2 = c3.GetChild("d2");
        var x = b2.GetChild("x");
        var y = x.GetChild("y");
        var z = y.GetChild("z");

        // redundant
        b1.BaseTokenSet.Should().Be(a);
        b2.BaseTokenSet.Should().Be(a);
        c1.BaseTokenSet.Should().Be(b1);
        c2.BaseTokenSet.Should().Be(b1);
        c3.BaseTokenSet.Should().Be(b1);
        d1.BaseTokenSet.Should().Be(c3);
        d2.BaseTokenSet.Should().Be(c3);
        x.BaseTokenSet.Should().Be(b2);
        y.BaseTokenSet.Should().Be(x);
        z.BaseTokenSet.Should().Be(y);
        
        
        
        a.TokenSetName.Should().Be("a");
        a.Children.Count().Should().Be(2);
        b1.Children.Count().Should().Be(3);
        c3.Children.Count().Should().Be(2);

        PrintTokenSetHierarchy(a, 0);
    }
    
    private void PrintTokenSetHierarchy(TokenSetComposed3 tokenSet, int level)
    {
        var space = new string(' ', level * 3);
        _testOutputHelper.WriteLine(space + tokenSet.TokenSetName);
        foreach (var child in tokenSet.Children)
        {
            PrintTokenSetHierarchy(child, level + 1);
        }
    }}