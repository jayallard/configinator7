using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Allard.Json.Tests.Unit;

public class JsonUtilityTests
{
    /*
     * TODO: variable not found, circular references, etc
     * only happy path so far
     */

    [Fact]
    public void GetVariables()
    {
        var value = new
        {
            FirstName = "$$first$$",
            LastName = "$$last$$",
            Stuff = new
            {
                Yabba = "$$first$$",
                Dabba = "$$last$$",
                Do = "$$object$$",
                PersonalAttributes = new
                {
                    FavoriteColor = "$$color$$",
                    SomethingElse = "Hi there",
                    NestAgain = new
                    {
                        Bird = "$$fowl$$",
                        LastAgain = "$$Last$$"
                    }
                }
            }
        }.ToJObject();

        var variables = JsonUtility.GetVariables(value);
        variables.Count().Should().Be(8);
        variables.Should().Contain(new[]
        {
            new ValueTuple<string, string>("first", "FirstName"),
            new ValueTuple<string, string>("last", "LastName"),
            new ValueTuple<string, string>("first", "Stuff.Yabba"),
            new ValueTuple<string, string>("last", "Stuff.Dabba"),
            new ValueTuple<string, string>("object", "Stuff.Do"),
            new ValueTuple<string, string>("color", "Stuff.PersonalAttributes.FavoriteColor"),
            new ValueTuple<string, string>("fowl", "Stuff.PersonalAttributes.NestAgain.Bird"),
            new ValueTuple<string, string>("Last", "Stuff.PersonalAttributes.NestAgain.LastAgain")
        });
    }

    [Fact]
    public void GetTokenNames()
    {
        var value = new
        {
            FirstName = "$$first$$",
            LastName = "$$last$$",
            Stuff = new
            {
                Yabba = "$$first$$",
                Dabba = "$$last$$",
                Do = "$$object$$",
                PersonalAttributes = new
                {
                    FavoriteColor = "$$color$$",
                    SomethingElse = "Hi there",
                    NestAgain = new
                    {
                        Bird = "$$fowl$$",
                        LastAgain = "$$Last$$"
                    }
                }
            }
        }.ToJObject();

        var variables = JsonUtility.GetVariableNames(value);
        variables.Count.Should().Be(5);
        variables.Should().Contain(new[]
        {
            "last", "first", "object", "color", "fowl"
        });
    }

    [Fact]
    public void GetTokenNamesDeep()
    {
        // -----------------------------------------------------------
        // arrange
        // -----------------------------------------------------------
        // the json value that refers to variables
        var value = new
        {
            FirstName = "$$first$$",
            LastName = "$$last$$",
            Stuff = new
            {
                Yabba = "$$first$$",
                Dabba = "$$last$$",
                Do = "$$object$$",
                PersonalAttributes = new
                {
                    FavoriteColor = "$$color$$",
                    SomethingElse = "Hi there",
                    NestAgain = new
                    {
                        Bird = "$$fowl$$"
                    }
                }
            }
        }.ToJObject();

        // one of the variables
        var objectToken = new
        {
            Color = "$$color$$",
            Age = "$$age$$",
            Allergies = new
            {
                Primary = "$$allergies$$"
            }
        }.ToJObject();

        // all of the other variables
        var variables = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
        {
            {"allergies", "gold fish"},
            {"age", 13},
            {"First", "Santa"},
            {"last", "Claus"},
            {"COLOR", "Clear"},
            {"object", objectToken},
            {"notused1", "not used1"},
            {"notused2", "not used2"},
            {"notused3", "not used3"},
            {"notused4", "not used4"},
            {"fowl", "chicken"}
        };

        // -----------------------------------------------------------
        // act
        // -----------------------------------------------------------
        var usedVariables = JsonUtility.GetVariableNamesDeep(value, variables).ToList();

        // -----------------------------------------------------------
        // assert
        // -----------------------------------------------------------
        usedVariables.Count.Should().Be(7);
        usedVariables.Should().Contain(new[] {"allergies", "first", "last", "age", "color", "object", "fowl"});
    }
}