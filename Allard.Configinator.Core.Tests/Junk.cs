﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests;

public class Junk
{
    [Fact]
    public void Blah()
    {
        var nodes = new List<Node>
        {
            new("a"),
            new("b", "a"),
            new("c", "b"),
            new("d1", "c"),
            new("d2", "c"),
            new("x", "b"),
            new("y", "a"),
            new("z"),
            new("aa"),
            new("bb"),
            new("cc")
        };

        var nodeMap = nodes.ToDictionary(n => n.Name, n => n, StringComparer.OrdinalIgnoreCase);
        foreach (var node in nodes.Where(n => n.ParentName != null))
        {
            nodeMap[node.ParentName].Children.Add(node);
            node.Parent = nodeMap[node.ParentName];
        }

        var descendants = nodeMap["b"].GetDescendants();
        descendants.Count.Should().Be(4);
        descendants.Should().Contain("c");
        descendants.Should().Contain("d1");
        descendants.Should().Contain("d2");
        descendants.Should().Contain("x");

        nodeMap["c"].IsDescendantOfOrSelf("b").Should().BeTrue();
        nodeMap["d2"].IsDescendantOfOrSelf("b").Should().BeTrue();
        nodeMap["d2"].IsDescendantOfOrSelf("b").Should().BeTrue();
        nodeMap["x"].IsDescendantOfOrSelf("b").Should().BeTrue();
    }
}

public record Node(string Name, string? ParentName = null)
{
    public List<Node> Children { get; } = new();

    public Node? Parent { get; set; }

    public bool IsDescendantOfOrSelf(string name)
    {
        if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase)) return true;
        var parent = Parent;
        while (parent != null)
        {
            if (string.Equals(parent.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }
    
    public HashSet<string> GetDescendants()
    {
        var results = new List<string>();

        foreach (var child in Children)
        {
            results.Add(child.Name);
            results.AddRange(child.GetDescendants());
        }

        return results.ToHashSet();
    }
}