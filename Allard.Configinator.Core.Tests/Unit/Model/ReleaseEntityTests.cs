using System;
using System.Threading.Tasks;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class ReleaseEntityTests
{
    [Fact]
    public async Task CreateDeployment()
    {
        throw new NotImplementedException();
        // // arrange
        // var section = CreateTestSection();
        // var env = section.Environments.Single();
        // var release = await section.CreateReleaseAsync(env.Id, new ReleaseId(22), null, Schema1Id, JsonDocument.Parse("{}"));
        //
        // // act
        // var date = DateTime.Now;
        // var deployment = section.SetDeployed(env.Id, release.Id, NewDeploymentId(0), date);
        //
        // // assert
        // release.Deployments.Single().Should().Be(deployment);
        // deployment.IsDeployed.Should().BeTrue();
    }

    /// <summary>
    ///     An environment has multiple releases.
    ///     Each time a release is DEPLOYED, it is flagged as deployed.
    ///     When a new release is deployed, then the old release is no longer deployed.
    ///     Show that the release is no longer deployed, and that the specific deployment is no
    ///     longer deployed.
    ///     NOTE: releases can be redeployed. That's a different scenario not covered by this test.
    /// </summary>
    [Fact]
    public async Task SetToRemoveWhenUndeployed()
    {
        throw new NotImplementedException();
        // var section = CreateTestSection();
        //
        // var env = section.Environments.Single();
        // var release1 = await section.CreateReleaseAsync(env.Id, new ReleaseId(22), null, Schema1Id, JsonDocument.Parse("{}"));
        // var release2 = await section.CreateReleaseAsync(env.Id, new ReleaseId(23), null, Schema1Id, JsonDocument.Parse("{}"));
        //
        // release1.IsDeployed.Should().BeFalse();
        // release2.IsDeployed.Should().BeFalse();
        // section.SetDeployed(env.Id, release1.Id, new DeploymentId(1), DateTime.Now);
        //
        // // deploy release 1
        // release1.IsDeployed.Should().BeTrue();
        // release1.Deployments.Single().IsDeployed.Should().BeTrue();
        // release1.Deployments.Single().RemovedDate.Should().BeNull();
        // release1.Deployments.Single().RemoveReason.Should().BeNull();
        // release2.IsDeployed.Should().BeFalse();
        //
        // // deploy release 2
        // section.SetDeployed(env.Id, release2.Id, new DeploymentId(2), DateTime.Now);
        // release1.IsDeployed.Should().BeFalse();
        // release1.Deployments.Single().IsDeployed.Should().BeFalse();
        // release1.Deployments.Single().RemovedDate.Should().NotBeNull();
        // release1.Deployments.Single().RemoveReason.Should().NotBeNull();
        // release2.IsDeployed.Should().BeTrue();
        // release2.Deployments.Single().IsDeployed.Should().BeTrue();
        // release2.Deployments.Single().RemovedDate.Should().BeNull();
        // release2.Deployments.Single().RemoveReason.Should().BeNull();
    }
}