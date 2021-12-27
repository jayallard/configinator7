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
    public async Task AddHabitatFailsIfAlreadyExists()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", await GetSchemaTest1(), "/something", null);
        agg.AddHabitat("secret", "habitat1");

        // act
        // add it again. boom.
        var test = () => agg.AddHabitat("secret", "habitat1");
        
        // assert
        test.Should().ThrowExactly<InvalidOperationException>().WithMessage("The habitat already exists: habitat1");
    }

    [Fact]
    public async Task AddHabitat()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", await GetSchemaTest1(), "/something", null);
        
        // act
        agg.AddHabitat("secret", "habitat1");
        
        // assert
        agg.TemporarySecretExposure.Values.Single().Habitats.Single().HabitatId.Name.Should().Be("habitat1");
    }

    [Fact]
    public async Task AddSchemaToHabitat()
    {
        var agg = new SuperAggregate();
        agg.CreateSecret("boo", await GetSchemaTest1(), "boo", null);

        var newSchema = await GetSchemaTest2() with {Version = new SemanticVersion(1, 1, 0)};
        agg.AddSchema("boo", newSchema);

        agg.TemporarySecretExposure["boo"].Schemas.Count.Should().Be(2);
        agg.TemporarySecretExposure["boo"].Schemas.Last().Version.Should().Be(new SemanticVersion(1, 1, 0));
        
        agg.AddHabitat("boo", "h");
        agg.TemporarySecretExposure["boo"]
            .Habitats
            .Single()
            .Schemas.Count.Should().Be(0);
        
        agg.AddSchemaToHabitat("boo", "h", new SemanticVersion(1,1,0));
        agg.TemporarySecretExposure["boo"]
            .Habitats
            .Single()
            .Schemas.Count.Should().Be(1);
    }

    [Fact]
    public async Task SetValue()
    {
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", await GetSchemaTest1(), "path", null);
        agg.AddHabitat("secret", "dev1");

        agg.TemporarySecretExposure["secret"].Schemas.Single().Schema.Should().NotBeNull();
    }
}