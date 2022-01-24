using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Allard.Json.Tests.Unit;

public class JsonUtilityTests
{
    /*
     * TODO: token not found, circular references, etc
     * only happy path so far
     */

    [Fact]
    public void GetTokens()
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

        var tokens = JsonUtility.GetTokens(value);
        tokens.Count().Should().Be(8);
        tokens.Should().Contain(new[]
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

        var tokens = JsonUtility.GetTokenNames(value);
        tokens.Count.Should().Be(5);
        tokens.Should().Contain(new string[]
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
        // the json value that refers to tokens
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

        // one of the tokens
        var objectToken = new
        {
            Color = "$$color$$",
            Age = "$$age$$",
            Allergies = new
            {
                Primary = "$$allergies$$"
            }
        }.ToJObject();

        // all of the other tokens
        var tokens = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase)
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
        var usedTokens = JsonUtility.GetTokenNamesDeep(value, tokens).ToList();
        
        // -----------------------------------------------------------
        // assert
        // -----------------------------------------------------------
        usedTokens.Count.Should().Be(7);
        usedTokens.Should().Contain(new [] {"allergies", "first", "last", "age", "color", "object", "fowl"});
    }
}