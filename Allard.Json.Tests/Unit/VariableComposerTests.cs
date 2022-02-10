using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Json.Tests.Unit;

public class VariableComposerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public VariableComposerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void VariableHierarchy()
    {
        var sets = new[]
        {
            new VariableSet
            {
                VariableSetName = "a",
                Variables =
                {
                    {"a", "from a"}
                }
            },
            new VariableSet
            {
                VariableSetName = "b",
                BaseVariableSetName = "a",
                Variables =
                {
                    {"b", "from b"}
                }
            },
            new VariableSet
            {
                VariableSetName = "c",
                BaseVariableSetName = "b",
                Variables =
                {
                    {"a", "from c"}
                }
            },
            new VariableSet
            {
                VariableSetName = "d",
                BaseVariableSetName = "c",
            },
        };

        var a = VariableSetComposer.Compose(sets, "a");
        var b = a.GetChild("b");
        var c = b.GetChild("c");
        var d = c.GetChild("d");
        
        // it's defined in A
        var variableAFromA = a.GetToken("a");
        variableAFromA.Origin.Should().Be(VariableOrigin.Defined);

        var variableAFromB = b.GetToken("a");
        variableAFromB.Origin.Should().Be(VariableOrigin.Inherited);

        var variableAFromC = c.GetToken("a");
        variableAFromC.Origin.Should().Be(VariableOrigin.Override);

        var variableAFromD = d.GetToken("a");
        variableAFromD.Origin.Should().Be(VariableOrigin.Inherited);
    }

    [Fact]
    public void AllKeys()
    {
        var sets = new[]
        {
            new VariableSet
            {
                VariableSetName = "a",
                Variables =
                {
                    {"a", "from a"}
                }
            },
            new VariableSet
            {
                VariableSetName = "b",
                BaseVariableSetName = "a",
                Variables =
                {
                    {"b", "from b"}
                }
            },
            new VariableSet
            {
                VariableSetName = "c",
                BaseVariableSetName = "b",
                Variables =
                {
                    {"a", "from c"},
                    {"c", "from c"}
                }
            },
            new VariableSet
            {
                VariableSetName = "d",
                BaseVariableSetName = "c",
                Variables =
                {
                }
            },
            new VariableSet
            {
                VariableSetName = "e",
                BaseVariableSetName = "d",
                Variables =
                {
                    {"e", "from e"}
                }
            },
        };

        var a = VariableSetComposer.Compose(sets, "a");
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
    public void VariableSetHierarchy()
    {
        var sets = new[]
        {
            new VariableSet {VariableSetName = "a"},
            new VariableSet {VariableSetName = "b1", BaseVariableSetName = "a"},
            new VariableSet {VariableSetName = "b2", BaseVariableSetName = "a"},
            new VariableSet {VariableSetName = "c1", BaseVariableSetName = "b1"},
            new VariableSet {VariableSetName = "c2", BaseVariableSetName = "b1"},
            new VariableSet {VariableSetName = "c3", BaseVariableSetName = "b1"},
            new VariableSet {VariableSetName = "d1", BaseVariableSetName = "c3"},
            new VariableSet {VariableSetName = "d2", BaseVariableSetName = "c3"},
            new VariableSet {VariableSetName = "x", BaseVariableSetName = "b2"},
            new VariableSet {VariableSetName = "y", BaseVariableSetName = "x"},
            new VariableSet {VariableSetName = "z", BaseVariableSetName = "y"},
        };

        var a = VariableSetComposer.Compose(sets, "a");
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
        b1.BaseVariableSet.Should().Be(a);
        b2.BaseVariableSet.Should().Be(a);
        c1.BaseVariableSet.Should().Be(b1);
        c2.BaseVariableSet.Should().Be(b1);
        c3.BaseVariableSet.Should().Be(b1);
        d1.BaseVariableSet.Should().Be(c3);
        d2.BaseVariableSet.Should().Be(c3);
        x.BaseVariableSet.Should().Be(b2);
        y.BaseVariableSet.Should().Be(x);
        z.BaseVariableSet.Should().Be(y);
        
        
        
        a.VariableSetName.Should().Be("a");
        a.Children.Count().Should().Be(2);
        b1.Children.Count().Should().Be(3);
        c3.Children.Count().Should().Be(2);

        PrintVariableSetHierarchy(a, 0);
    }
    
    private void PrintVariableSetHierarchy(VariableSetComposed variableSet, int level)
    {
        var space = new string(' ', level * 3);
        _testOutputHelper.WriteLine(space + variableSet.VariableSetName);
        foreach (var child in variableSet.Children)
        {
            PrintVariableSetHierarchy(child, level + 1);
        }
    }}