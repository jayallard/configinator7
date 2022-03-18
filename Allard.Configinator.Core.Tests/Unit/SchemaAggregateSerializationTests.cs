﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Allard.Json;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Allard.Configinator.Core.Tests.Unit;

public class SchemaAggregateSerializationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchemaAggregateSerializationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void SerializeAndDeserializeSchemaAggregate()
    {
        // arrange
        var schema = new SchemaAggregate(
            new SchemaId(3),
            new SectionId(5),
            "Development",
            "/na/a/b/c",
            new SchemaName("boo/1.0.0"),
            "description",
            TestSchema());
        schema.Promote("staging");
        schema.Promote("production");

        // act
        EnsureSerializesAndDeserializesToSameThing(schema, _testOutputHelper);
    }

    [Fact]
    public async Task SemanticVersionTest()
    {
        var version = SemanticVersion.Parse("1.0.0-prerelease+3333");
        EnsureSerializesAndDeserializesToSameThing(version, _testOutputHelper);
    }
}
