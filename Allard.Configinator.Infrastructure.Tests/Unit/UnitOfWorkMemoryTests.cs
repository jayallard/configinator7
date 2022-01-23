using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Infrastructure.Tests.Unit;

public class UnitOfWorkMemoryTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISectionRepository _sectionRepository;

    public UnitOfWorkMemoryTests(IUnitOfWork unitOfWork, ISectionRepository sectionRepository)
    {
        _unitOfWork = unitOfWork;
        _sectionRepository = sectionRepository;
    }

    /// <summary>
    /// Add a section to UOW.
    /// See that it persisted in the repo when saved.
    /// </summary>
    [Fact]
    public async Task UowWritesToRepositoryOnSave()
    {
        // arrange
        var section = new SectionEntity(new SectionId(0), "name", "path");
        
        // act
        await _unitOfWork.AddSectionAsync(section);
        
        // assert
        (await _sectionRepository.FindAsync(new AllSections())).Should().BeEmpty();
        await _unitOfWork.SaveChangesAsync();
        (await _sectionRepository.FindAsync(new AllSections())).Count().Should().Be(1);
    }
}