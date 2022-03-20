using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Infrastructure.Memory.Tests;

public class IdentityServiceMemoryTests
{
    [Fact]
    public async Task ShouldWork()
    {
        var service = new IdentityServiceMemory();

        (await service.GetIdAsync<SectionId>()).Id.Should().Be(0);
        (await service.GetIdAsync<SectionId>()).Id.Should().Be(1);
        (await service.GetIdAsync<SectionId>()).Id.Should().Be(2);
        (await service.GetIdAsync<SectionId>()).Id.Should().Be(3);

        (await service.GetIdAsync<SchemaId>()).Id.Should().Be(0);
        (await service.GetIdAsync<SchemaId>()).Id.Should().Be(1);
        (await service.GetIdAsync<SchemaId>()).Id.Should().Be(2);
        (await service.GetIdAsync<SchemaId>()).Id.Should().Be(3);
    }

    [Fact]
    public void Concurrency()
    {
        var service = new IdentityServiceMemory();
        var threads = Enumerable
            .Range(0, 50)
            .Select(i =>
            {
                var thread = new Thread(() =>
                {
                    var _ = service.GetIdAsync<SchemaId>().Result;
                });
                thread.Start();
                return thread;
            });
        foreach (var thread in threads) thread.Join();
    }
}