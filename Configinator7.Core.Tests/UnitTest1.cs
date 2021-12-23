using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configinator7.Core.Model;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
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

    private static async Task<JsonSchema> GetSchema(string name)
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", name);
        return await JsonSchema.FromFileAsync(file);
    }
    
    /// <summary>
    /// firstname, lastname required
    /// age optional
    /// </summary>
    /// <returns></returns>
    private static Task<JsonSchema> GetSchemaTest1() => GetSchema("test1.json");

    /// <summary>
    /// firstname, lastname, age required
    /// </summary>
    /// <returns></returns>
    private static Task<JsonSchema> GetSchemaTest2() => GetSchema("test2.json");

    /// <summary>
    /// firstname, lastname required
    /// age, favorite color optional
    /// </summary>
    /// <returns></returns>
    private static Task<JsonSchema> GetSchemaTest3() => GetSchema("test3.json");

    [Fact]
    public async Task AddHabitatFailsIfValueDoesntValidate()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", "/something", await GetSchemaTest1());

        // schema requires FirstName and LastName. This has neither.
        // boom!
        var badValue = new JObject();
        
        // act
        var test = () => agg.AddHabitat("secret", "habitat", badValue);
        
        // assert
        test.Should().ThrowExactly<InvalidOperationException>().WithMessage("Value is invalid. Can't be assigned.");
    }

    [Fact]
    public async Task AddHabitatSucceedsIfValueValidates()
    {
        // arrange
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", "/something", await GetSchemaTest1());

        // schema requires FirstName and LastName. This is good.
        var goodValue = JObject.FromObject(new {firstName = "Santa", lastName = "Claus"});
        
        // act
        agg.AddHabitat("secret", "habitat", goodValue);
        
        // assert
        agg.TemporarySecretExposure.Values
            .Single() // only one list of secrets
            .Single() // only one secret on the list
            .Habitats.Count.Should().Be(1);
    }

    [Fact]
    public async Task ChangeSchemaFailsIfValidationFails()
    {
        // -----------------------
        // arrange
        // -----------------------
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", "/something", await GetSchemaTest1());

        // schema requires FirstName and LastName. This is good.
        var goodValue = JObject.FromObject(new {firstName = "Santa", lastName = "Claus"});
        agg.AddHabitat("secret", "habitat", goodValue);

        // -----------------------
        // act
        // -----------------------
        // now update to a different schema. this requires AGE, which 
        // isn't in the habitat's current value, so it wil fail.
        var newSchema = await GetSchemaTest2();
        var test = () => agg.UpdateSecretSchema("secret", newSchema);
        
        // -----------------------
        // assert
        // -----------------------
        test.Should().Throw<SchemaValidationFailedException>();
    }
    
    [Fact]
    public async Task ChangeSchemaSucceedsIfValidationSucceeds()
    {
        // -----------------------
        // arrange
        // -----------------------
        var agg = new SuperAggregate();
        agg.CreateSecret("secret", "/something", await GetSchemaTest1());

        // schema requires FirstName and LastName. This is good.
        var goodValue = JObject.FromObject(new {firstName = "Santa", lastName = "Claus"});
        agg.AddHabitat("secret", "habitat", goodValue);

        // -----------------------
        // act
        // -----------------------
        // now update to a different schema, which has a new optional
        // property, "favorite color". schema validation passes,
        // to the operation succeeds.
        // isn't in the habitat's current value, so it wil fail.
        var newSchema = await GetSchemaTest3();
        agg.UpdateSecretSchema("secret", newSchema);
        
        // -----------------------
        // assert
        // -----------------------
        var secretHistory = agg.TemporarySecretExposure.Values.Single();
        secretHistory.Count.Should().Be(2);
        var secret = secretHistory.Last();
        secret.Id.Should().Be(new SecretId("secret", 2));
        secret.Habitats.Count.Should().Be(1);
    }
}