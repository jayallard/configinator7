using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit;

public class OrganizationPathHierarchy
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OrganizationPathHierarchy(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void FlattenOne()
    {
        var paths = new[] {"a/b/c/d/e"};
        var nodes = CreateHierarchy(paths);
        var flattened = Flatten(nodes);

        flattened.Values.Single().Path.Should().Be("/a/b/c/d/e");
    }

    [Fact]
    public void FlattenTwo()
    {
        var paths = new[]
        {
            "/a/b/c/d/e",
            "/a/b/c/d/e/f/g/h"
        };
        var nodes = CreateHierarchy(paths);
        // Print(nodes, 0,  0);
        // _testOutputHelper.WriteLine("---------------");
        var flattened = Flatten(nodes);
        // Print(flattened, 0, 0);
        flattened.Count.Should().Be(1);
        flattened.Values.Single().Path.Should().Be("/a/b/c/d/e");
        flattened.Values.Single().Name.Should().Be("e");

        flattened.Values.Single().Children.Values.Single().Name.Should().Be("h");
        flattened.Values.Single().Children.Values.Single().Path.Should().Be("/a/b/c/d/e/f/g/h");
    }

    [Fact]
    public void Blah()
    {
        var paths = new[]
        {
            "/a/b/c/d",
            "/a/b/c/d/e/f",
            "/a/b/c/x",
            "/a/b/c/x/y/z/1/2/3/4",
            "/a/b/c/j/k"
        };

        // var sorts = paths.OrderBy(p => p);
        // foreach (var p in paths)
        // {
        //     _testOutputHelper.WriteLine(p);
        // }

        var nodes = CreateHierarchy(paths);
        nodes = Flatten(nodes);
        Print(nodes, 0, 0);
    }

    private void Print(ConcurrentDictionary<string, Node> print, int level, int spacesPerLevel)
    {
        var spaces = new string(' ', level * spacesPerLevel);
        foreach (var (k, v) in print.OrderBy(kv => kv.Key))
        {
            _testOutputHelper.WriteLine(spaces + v.Path);
            Print(v.Children, level + 1, spacesPerLevel);
        }
    }

    public record Node(string Name, string Path, bool HasValue)
    {
        public ConcurrentDictionary<string, Node> Children { get; } = new();
    }

    private static ConcurrentDictionary<string, Node> CreateHierarchy(IEnumerable<string> paths)
    {
        // hacky McHack face
        // super inefficient... just push through
        var pathSet = paths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var results = new ConcurrentDictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in pathSet)
        {
            var currentPath = "";
            var current = results;
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                currentPath += "/" + part;
                var cp = currentPath;
                current = current.GetOrAdd(part, p => new Node(part, cp, pathSet.Contains(cp))).Children;
            }
        }

        return results;
    }

    // reverses a hierarchy back to flat
    private static ConcurrentDictionary<string, Node> Flatten(ConcurrentDictionary<string, Node> nodes)
    {
        var result = new ConcurrentDictionary<string, Node>();
        foreach (var kv in nodes)
        {
            var flattened = Flatten(kv.Value);
            result[flattened.Path] = flattened;
        }

        return result;
    }

    private static Node Flatten(Node node)
    {
        while (true)
        {
            var newNode = new Node(node.Name, node.Path, node.HasValue);
            foreach (var c in node.Children)
            {
                var flattened = Flatten(c.Value);
                newNode.Children[flattened.Path] = flattened;
            }

            if (newNode.HasValue || newNode.Children.Count != 1) return newNode;
            node = node.Children.Values.Single();
        }
    }
}