using System;
using System.Threading.Tasks;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.Model;

public class EnvironmentEntityTests
{
    [Fact]
    public async Task AddRelease()
    {
        throw new NotImplementedException();
        // // arrange
        // var section = CreateTestSection();
        // var environment1 = section.GetEnvironment("test1");
        //
        // // act
        // var release = await section.CreateReleaseAsync(
        //     environment1.Id,
        //     NewReleaseId(0),
        //     null,
        //     Schema1Id,
        //     JsonDocument.Parse("{}"));
        //
        // // assert
        // environment1.Releases.Single().Should().Be(release);
    }

    [Fact]
    public async Task AddReleaseThrowsExceptionIfReleaseIdExists()
    {
        throw new NotImplementedException();
        // // arrange
        // var section = CreateTestSection();
        // var environment1 = section.GetEnvironment("test1");
        //
        // // act
        // await section.CreateReleaseAsync(
        //     environment1.Id,
        //     NewReleaseId(0),
        //     null,
        //     Schema1Id,
        //     JsonDocument.Parse("{}"));
        //
        // var test = () =>
        // {
        //     section.CreateReleaseAsync(
        //             environment1.Id,
        //             NewReleaseId(0),
        //             null,
        //             Schema1Id,
        //             JsonDocument.Parse("{}"))
        //         // TODO: how do we test an exception async?
        //         .Wait();
        // };
        //
        // // assert
        // test
        //     .Should()
        //     .ThrowExactly<AggregateException>()
        //     .WithInnerException<InvalidOperationException>()
        //     .WithMessage("Release already exists. Id=0");
    }
}