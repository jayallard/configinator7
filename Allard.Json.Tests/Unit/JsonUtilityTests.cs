using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Json.Tests.Unit;

public class JsonUtilityTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public JsonUtilityTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    /*
     * TODO: variable not found, circular references, etc
     * only happy path so far
     */


    [Fact]
    public async Task ReplaceValues()
    {
        // arrange - setup a value with a bunch of values
        var model = new
        {
            floatValue = "$$float$$",
            number = "$$number$$",
            boolTrue = "$$true$$",
            boolFalse = "$$false$$",
            first = "$$first$$",
            last = "$$last$$",
            full = "$$last$$, $$first$$ $$middle$$",
            connection = "$$connection$$",
            embeddedStuff = " $$first$$ $$true$$ $$number$$ $$float$$ "
        }.ToJObject();

        var variables = new Dictionary<string, JToken>
        {
            {"first", "santa"},
            {"last", "claus"},
            {"middle", "eugene"},
            {
                "connection", new
                {
                    userId = "$$userid$$",
                    password = "$$password$$"
                }.ToJObject()
            },
            {"userid", "the user"},
            {"password", "the password"},
            {"true", true},
            {"false", false},
            {"number", 1234},
            {"float", 88.3}
        };

        var actual = await JsonUtility.ResolveAsync(model, variables);
        var expected = new
        {
            floatValue = 88.3,
            number = 1234,
            boolTrue = true,
            boolFalse = false,
            first = "santa",
            last = "claus",
            full = "claus, santa eugene",
            embeddedStuff = " santa True 1234 88.3 ",
            connection = new
            {
                userId = "the user",
                password = "the password"
            }
        }.ToJObject();

        // _testOutputHelper.WriteLine(expected.Root.ToString());
        // _testOutputHelper.WriteLine(actual.Root.ToString());
        JToken.DeepEquals(actual, expected).Should().BeTrue();
    }

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
                MultiValue = "$$a$$ is a $$b$$, $$c$$",
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

        var variables = JsonUtility.GetVariables(value).ToList();
        variables.Count.Should().Be(11);
        variables.Should().Contain(new[]
        {
            new ValueTuple<string, string>("first", "FirstName"),
            new ValueTuple<string, string>("last", "LastName"),
            new ValueTuple<string, string>("first", "Stuff.Yabba"),
            new ValueTuple<string, string>("last", "Stuff.Dabba"),
            new ValueTuple<string, string>("object", "Stuff.Do"),
            new ValueTuple<string, string>("color", "Stuff.PersonalAttributes.FavoriteColor"),
            new ValueTuple<string, string>("fowl", "Stuff.PersonalAttributes.NestAgain.Bird"),
            new ValueTuple<string, string>("Last", "Stuff.PersonalAttributes.NestAgain.LastAgain"),
            new ValueTuple<string, string>("a", "Stuff.MultiValue"),
            new ValueTuple<string, string>("b", "Stuff.MultiValue"),
            new ValueTuple<string, string>("c", "Stuff.MultiValue")
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