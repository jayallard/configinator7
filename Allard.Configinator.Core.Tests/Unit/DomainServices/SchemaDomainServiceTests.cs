using System;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.DomainServices;

public class SchemaDomainServiceTests
{
    private readonly SchemaDomainService _schemaService;
    private readonly SectionDomainService _sectionService;

    public SchemaDomainServiceTests(SchemaDomainService schemaService, SectionDomainService sectionService)
    {
        _schemaService = schemaService;
        _sectionService = sectionService;
    }

    [Fact]
    public async Task CantCreateSchemaIfTheNameAlreadyExists()
    {
        var schema1 = await _schemaService.CreateSchemaAsync(
            null,
            "/test",
            new SchemaName("a/1.0.0"),
            null,
            TestUtility.TestSchema());

        var test = async () => await _schemaService.CreateSchemaAsync(
            null,
            "/test",
            new SchemaName("a/1.0.0"),
            null,
            TestSchema());

        await test.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The schema name is already in use. SchemaName=a/1.0.0");
    }

    /// <summary>
    /// The schema must have at least one property.
    /// </summary>
    [Fact]
    public async Task CantCreateIfSchemaIsInvalid()
    {
        var test = async () => await _schemaService.CreateSchemaAsync(
            null,
            "/test",
            new SchemaName("b/1.0.0"),
            null,
            EmptyDoc());

        await test.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The schema doesn't define any properties. SchemaName=b/1.0.0");
    }

    /// <summary>
    /// If the schema in namespace /a/b/c, it can only reference
    /// other schemas in that namespace or one of it's parents.
    /// </summary>
    /// <param name="schemaHere"></param>
    /// <param name="refersToSchemaHere"></param>
    /// <param name="isValid"></param>
    /// <exception cref="Exception"></exception>
    [Theory]
    [InlineData("/a/b/c", "/", true)]
    [InlineData("/a/b/c", "/a", true)]
    [InlineData("/a/b/c", "/a/b", true)]
    [InlineData("/a/b/c", "/a/b/c", true)]
    [InlineData("/a/b/c", "/a/b/c/d", false)]
    [InlineData("/a/b/c", "/aa", false)]
    public async Task SchemaReferencesMustBeSelfOrAscendant(string schemaHere, string refersToSchemaHere, bool isValid)
    {
        //language=json
        var referencedSchema =
            JsonDocument.Parse(
                "{\n  \"properties\": {\n    \"smashing\": {\n      \"type\": \"string\"\n    }\n  }\n}");
        var schema =
            JsonDocument.Parse(
                "{\n  \"properties\": {\n    \"something\": {\n      \"$ref\": \"reference/2.2.2\"\n    }\n  }\n}");
        
        // create the reference schema
        await _schemaService.CreateSchemaAsync(
            null,
            refersToSchemaHere,
            new SchemaName("reference/2.2.2"),
            null,
            referencedSchema);

        try
        {
            // create the schema that users the reference schema
            // if the reference schema is the same namespace, or an
            // ascendant, it will work.
            await _schemaService.CreateSchemaAsync(
                null,
                schemaHere,
                new SchemaName("base/1.1.1"),
                null,
                schema);

            if (isValid) return;
            throw new Exception("Should've failed.");
        }
        catch (InvalidOperationException ex)
        {
            if (!isValid) return;
            throw new Exception("Should not have failed");
        }
    }

    /// <summary>
    /// Given a schema with a reference to another schema: schema and reference
    /// Both schemas are created in the lowest environment type automatically. (IE: development)
    /// The schema can't be promoted to the next environment type until the reference is.
    /// This is fully recursive; references to references to references;
    /// None can be promoted until all it's references have been.
    /// </summary>
    [Fact]
    public async Task SchemaCantBePromotedIfRefersToSchemaInLesserEnvironmentType()
    {
        //language=json
        var referencedSchema =
            JsonDocument.Parse(
                "{\n  \"properties\": {\n    \"smashing\": {\n      \"type\": \"string\"\n    }\n  }\n}");
        var schema =
            JsonDocument.Parse(
                "{\n  \"properties\": {\n    \"something\": {\n      \"$ref\": \"reference/2.2.2\"\n    }\n  }\n}");


        // create the reference schema
        var referenceAggregate = await _schemaService.CreateSchemaAsync(
            null,
            "/a/b/c",
            new SchemaName("reference/2.2.2"),
            null,
            referencedSchema);

        // create the schema that users the reference schema
        // if the reference schema is the same namespace, or an
        // ascendant, it will work.
        var schemaAggregate = await _schemaService.CreateSchemaAsync(
            null,
            "/a/b/c",
            new SchemaName("base/1.1.1"),
            null,
            schema);

        // will fail. can't promote the schema to STAGING, 
        // because REFERENCE doesn't exist in staging.
        var test = async () => await _schemaService.PromoteSchemaAsync(schemaAggregate.SchemaName, "Staging");
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The schema, 'base/1.1.1', can't be promoted to 'Staging'. It refers to 'SchemaName { Version = 2.2.2, Name = reference, FullName = reference/2.2.2 }', which isn't assigned to 'Staging'.");
    }

    /// <summary>
    /// A section's schema must be in the same namespace as the exception.
    /// </summary>
    [Fact]
    public async Task ThrowExceptionIfSectionSchemaNotInTheSameNaemspaceAsTheSection()
    {
        var section = await _sectionService.CreateSectionAsync("/a/b/c", "test-section");
        var test = async () => await _schemaService.CreateSchemaAsync(section.Id,
            "/a/b",
            new SchemaName("awesome/1.0.0"),
            null,
            TestSchema());
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The schema must be in the same namespace as the section.*");
    }

    [Fact]
    public async Task CreateSchemaSection()
    {
        var section = await _sectionService.CreateSectionAsync("/a/b/c", "test-section");
        var schema = await _schemaService.CreateSchemaAsync(section.Id,
            "/a/b/c",
            new SchemaName("awesome/1.0.0"),
            null,
            TestSchema());
        schema.SectionId.Should().Be(section.Id);
    }
}