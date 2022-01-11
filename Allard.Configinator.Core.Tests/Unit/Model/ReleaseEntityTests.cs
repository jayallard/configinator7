using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
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
        var release = await env.CreateReleaseAsync(NewReleaseId(0), null, Schema1Id, JsonDocument.Parse("{}"));
        var date = DateTime.Now;
        
        // act
        var deployment = release.SetDeployed(NewDeploymentId(0), date);

        // assert
        release.Deployments.Single().Should().Be(deployment);
        deployment.IsDeployed.Should().BeTrue();
    }
}