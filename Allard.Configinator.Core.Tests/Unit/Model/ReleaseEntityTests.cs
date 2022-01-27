using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

using static IdUtility;
using static ModelTestUtility;

public class ReleaseEntityTests
{
    [Fact]
    public async Task CreateDeployment()
    {
        // arrange
        var section = CreateTestSection();
        var env = section.Environments.Single();
        var release = await section.CreateReleaseAsync(env.Id, new ReleaseId(22), null, Schema1Id, JsonDocument.Parse("{}"));
        var date = DateTime.Now;

        // act
        var deployment = section.SetDeployed(env.Id, release.Id, NewDeploymentId(0), date);

        // assert
        release.Deployments.Single().Should().Be(deployment);
        deployment.IsDeployed.Should().BeTrue();
    }
}