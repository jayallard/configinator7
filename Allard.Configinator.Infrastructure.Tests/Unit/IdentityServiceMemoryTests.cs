using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Infrastructure.Tests.Unit;

public class IdentityServiceMemoryTests
{
    [Fact]
    public async Task ShouldWork()
    {
        var service = new IdentityServiceMemory();

        (await service.GetId<SectionId>()).Id.Should().Be(0);
        (await service.GetId<SectionId>()).Id.Should().Be(1);
        (await service.GetId<SectionId>()).Id.Should().Be(2);
        (await service.GetId<SectionId>()).Id.Should().Be(3);
        
        (await service.GetId<SchemaId>()).Id.Should().Be(0);
        (await service.GetId<SchemaId>()).Id.Should().Be(1);
        (await service.GetId<SchemaId>()).Id.Should().Be(2);
        (await service.GetId<SchemaId>()).Id.Should().Be(3);
    }
}