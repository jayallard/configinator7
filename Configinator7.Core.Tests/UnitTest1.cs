using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configinator7.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;

namespace Configinator7.Core.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static async Task<ConfigurationSchema> GetSchema(string name)
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", name);
        var schema = await JsonSchema.FromFileAsync(file);
        return new ConfigurationSchema(new SemanticVersion(1, 0, 0), schema);
    }

    /// <summary>
    /// firstname, lastname required
    /// age optional
    /// </summary>
    /// <returns></returns>
    private static Task<ConfigurationSchema> GetSchemaTest1() => GetSchema("test1.json");

    private static JObject GetValidJsonTest1() => JObject.FromObject(new {firstName = "Santa", lastName = "Claus"});

    /// <summary>
    /// firstname, lastname, age required
    /// </summary>
    /// <returns></returns>
    private static Task<ConfigurationSchema> GetSchemaTest2() => GetSchema("test2.json");

    /// <summary>
    /// firstname, lastname required
    /// age, favorite color optional
    /// </summary>
    /// <returns></returns>
    private static Task<ConfigurationSchema> GetSchemaTest3() => GetSchema("test3.json");

    [Fact]
    public async Task AddEnvironmentFailsIfAlreadyExists()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSection("section1", await GetSchemaTest1(), "/something", null);
        agg.AddEnvironment("section1", "env1");

        // act
        // add it again. boom.
        var test = () => agg.AddEnvironment("section1", "env1");
        
        // assert
        test.Should().ThrowExactly<InvalidOperationException>().WithMessage("The environment already exists: env1");
    }

    [Fact]
    public async Task AddEnvironment()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSection("section1", await GetSchemaTest1(), "/something", null);
        
        // act
        agg.AddEnvironment("section1", "env1");
        
        // assert
        agg.TemporaryExposure.Values.Single().Environments.Single().EnvironmentId.Name.Should().Be("env1");
    }

    [Fact]
    public async Task SetValue()
    {
        var agg = new SuperAggregate();
        agg.CreateSection("section1", await GetSchemaTest1(), "path", null);
        agg.AddEnvironment("section1", "dev1");

        agg.TemporaryExposure["section1"].Schemas.Single().Schema.Should().NotBeNull();
    }
}