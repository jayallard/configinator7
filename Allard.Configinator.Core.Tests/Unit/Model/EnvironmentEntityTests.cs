using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Model.State;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;
using Xunit;
using static Allard.Configinator.Core.IdUtility;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class EnvironmentEntityTests
{
    private static readonly SemanticVersion _schema1Id = new(1, 0, 0);

    private static SectionEntity CreateTestSection()
    {
        {
            var schema = new ConfigurationSchema(_schema1Id, JsonSchema.CreateAnySchema());
            var section = new SectionEntity(SectionId(0), "s", "p", schema);
            section.AddEnvironment(EnvironmentId(0), "test1");
            return section;
        }
    }

    [Fact]
    public async Task AddRelease()
    {
        // arrange
        var environment1 = CreateTestSection().GetEnvironment("test1");

        // act
        var release = await environment1.CreateReleaseAsync(
            ReleaseId(0),
            null,
            _schema1Id,
            JObject.Parse("{}"));

        // assert
        environment1.Releases.Single().Should().Be(release);
    }

    [Fact]
    public async Task AddReleaseThrowsExceptionIfReleaseIdExists()
    {
        // arrange
        var environment1 = CreateTestSection().GetEnvironment("test1");

        // act
        var release = await environment1.CreateReleaseAsync(
            ReleaseId(0),
            null,
            _schema1Id,
            JObject.Parse("{}"));

        var test = () =>
        {
            environment1.CreateReleaseAsync(
                    ReleaseId(0),
                    null,
                    _schema1Id,
                    JObject.Parse("{}"))
                // TODO: how do we test an exception async?
                .Wait();
        };

        // assert
        test
            .Should()
            .ThrowExactly<AggregateException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Release id already exists: 0");
    }
}