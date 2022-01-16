﻿using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using NJsonSchema;
using NuGet.Versioning;
using static Allard.Configinator.Core.IdUtility;

namespace Allard.Configinator.Core.Tests;

public static class ModelTestUtility
{
    public static readonly SchemaId Schema1Id = NewSchemaId(0);
    public static readonly SemanticVersion Schema1Version = new(1, 0, 0);

    public static SectionEntity CreateTestSection()
    {
        {
            var schema = new SchemaEntity(Schema1Id, Schema1Version, JsonSchema.CreateAnySchema());
            var section = new SectionEntity(NewSectionId(0), "s", "p", schema);
            section.AddEnvironment(NewEnvironmentId(0), "test1");
            return section;
        }
    }
}