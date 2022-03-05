using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Infrastructure.Tests.Unit;

public class UnitOfWorkMemoryTests
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkMemoryTests(IUnitOfWork unitOfWork, ISectionRepository sectionRepository)
    {
        _unitOfWork = unitOfWork;
        _sectionRepository = sectionRepository;
    }

    /// <summary>
    ///     Add a section to UOW.
    ///     See that it persisted in the repo when saved.
    /// </summary>
    [Fact]
    public async Task UowWritesToRepositoryOnSave()
    {
        // arrange
        var section = new SectionAggregate(new SectionId(27), "Development", "ns", "name");

        // act
        await _unitOfWork.Sections.AddAsync(section);

        // assert
        (await _sectionRepository.FindAsync(new All())).Should().BeEmpty();
        await _unitOfWork.SaveChangesAsync();
        (await _sectionRepository.FindAsync(new All())).Count().Should().Be(1);
    }
}