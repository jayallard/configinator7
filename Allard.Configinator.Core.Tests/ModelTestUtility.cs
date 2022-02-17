using System;
using System.Text.Json;
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Tests;

public static class ModelTestUtility
{
    public static readonly SectionSchemaId Schema1Id = NewSchemaId(0);
    public static readonly SemanticVersion Schema1Version = new(1, 0, 0);

    public static SectionAggregate CreateTestSection()
    {
        var section = new SectionAggregate(NewSectionId(0), "development", "s", "p");
        //section.AddEnvironment(NewEnvironmentId(0), "test1");
        //return section;
        throw new NotImplementedException();
    }
}