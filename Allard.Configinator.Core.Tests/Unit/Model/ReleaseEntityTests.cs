using System;
using System.Linq;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;
using static Allard.Configinator.Core.Tests.TestUtility;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class ReleaseEntityTests
{
    [Fact]
    public void CreateDeployment()
    {
        // arrange
        var section = CreateTestSection();
        var env = section.Environments.Single();
        var release = section.CreateRelease(
            NewReleaseId(3),
            env.Id,
            new VariableSetId(15),
            new SchemaId(12),
            EmptyDoc(),
            EmptyDoc());

        // act
        var date = DateTime.Now;
        var deployment = section.SetDeployed(NewDeploymentId(0),
            release.Id,
            env.Id,
            EmptyDeploymentResult(), date);

        // assert
        release.Deployments.Single().Should().Be(deployment);
        deployment.Status.Should().Be(DeploymentStatus.Deployed);
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
    public void SetToRemovedWhenRemoved()
    {
        var section = CreateTestSection();

        var env = section.Environments.Single();
        var release1 = section.CreateRelease(NewReleaseId(22), env.Id, NewVariableSetId(28), NewSchemaId(29),
            EmptyDoc(), EmptyDoc());
        var release2 = section.CreateRelease(NewReleaseId(23), env.Id, new VariableSetId(29), new SchemaId(30),
            EmptyDoc(), EmptyDoc());

        release1.IsDeployed.Should().BeFalse();
        release2.IsDeployed.Should().BeFalse();
        section.SetDeployed(new DeploymentId(1), release1.Id, env.Id, EmptyDeploymentResult(), DateTime.Now);

        // deploy release 1
        release1.IsDeployed.Should().BeTrue();
        release1.Deployments.Single().Status.Should().Be(DeploymentStatus.Deployed);
        release1.Deployments.Single().RemovedDate.Should().BeNull();
        release1.Deployments.Single().RemoveReason.Should().BeNull();
        release2.IsDeployed.Should().BeFalse();

        // deploy release 2
        section.SetDeployed(NewDeploymentId(2), release2.Id, env.Id, EmptyDeploymentResult(), DateTime.Now);
        release1.IsDeployed.Should().BeFalse();
        release1.Deployments.Single().Status.Should().Be(DeploymentStatus.Removed);
        release1.Deployments.Single().RemovedDate.Should().NotBeNull();
        release1.Deployments.Single().RemoveReason.Should().NotBeNull();
        release2.IsDeployed.Should().BeTrue();
        release2.Deployments.Single().Status.Should().Be(DeploymentStatus.Deployed);
        release2.Deployments.Single().RemovedDate.Should().BeNull();
        release2.Deployments.Single().RemoveReason.Should().BeNull();
    }
}