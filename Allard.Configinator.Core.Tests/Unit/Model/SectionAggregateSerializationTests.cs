using System;
using System.Collections.Generic;
using Allard.Configinator.Core.Model;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class SectionAggregateSerializationTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SectionAggregateSerializationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SerializeAndDeserialize()
    {
        var section = new SectionAggregate(new SectionId(13), "development", "/ns/a/b", "section1");
        section.PromoteTo("staging");
        section.AddEnvironment(new EnvironmentId(22), "staging", "stg");
        section.AddEnvironment(new EnvironmentId(222), "production", "prod");
        section.CreateRelease(
            new ReleaseId(999),
            new EnvironmentId(22),
            new VariableSetId(27), new SchemaId(99),
            TestValue(),
            TestValue());

        var result = new DeploymentResult(true,
            new List<DeploymentResultMessage>
                    {new("source", "key", LogLevel.Critical, "boom", "exception")}
                .AsReadOnly());
        section.SetDeployed(
            new DeploymentId(1),
            new ReleaseId(999),
            new EnvironmentId(22),
            result,
            DateTime.Now,
            "notes");
        section.SetActiveDeploymentToRemoved(new EnvironmentId(22), new DeploymentId(22));

        EnsureSerializesAndDeserializesToSameThing(section, _testOutputHelper);
    }
}