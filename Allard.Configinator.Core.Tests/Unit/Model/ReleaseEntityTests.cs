using System;
using System.Linq;
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
        var release = await env.CreateReleaseAsync(ReleaseId(0), null, Schema1Id, JObject.Parse("{}"));
        var date = DateTime.Now;
        
        // act
        var deployment = release.SetDeployed(DeploymentId(0), date);

        // assert
        release.Deployments.Single().Should().Be(deployment);
        deployment.IsDeployed.Should().BeTrue();
    }
}