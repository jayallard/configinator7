using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using static Allard.Configinator.Core.IdUtility;
using static Allard.Configinator.Core.Tests.ModelTestUtility;
namespace Allard.Configinator.Core.Tests.Unit.Model;

public class EnvironmentEntityTests
{
    [Fact]
    public async Task AddRelease()
    {
        // arrange
        var environment1 = CreateTestSection().GetEnvironment("test1");

        // act
        var release = await environment1.CreateReleaseAsync(
            NewReleaseId(0),
            null,
            Schema1Id,
            JsonDocument.Parse("{}"));

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
            NewReleaseId(0),
            null,
            Schema1Id,
            JsonDocument.Parse("{}"));

        var test = () =>
        {
            environment1.CreateReleaseAsync(
                    NewReleaseId(0),
                    null,
                    Schema1Id,
                    JsonDocument.Parse("{}"))
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