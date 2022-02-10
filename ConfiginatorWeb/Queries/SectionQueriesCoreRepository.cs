using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesCoreRepository : ISectionQueries
{
    private readonly IUnitOfWork _unitOfWork;

    public SectionQueriesCoreRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SectionListItemDto>> GetSectionsListAsync(CancellationToken cancellationToken = default)
        => (await _unitOfWork.Sections.FindAsync(new AllSections(), cancellationToken))
            .Select(s => new SectionListItemDto(s.Id.Id, s.SectionName, s.OrganizationPath))
            .ToList();

    public async Task<SectionDto?> GetSectionAsync(long id, CancellationToken cancellationToken = default)
    {
        var section = await _unitOfWork.Sections.GetAsync(new SectionId(id), cancellationToken);
        return await CreateSectionDto(section, cancellationToken);
    }

    public async Task<SectionDto> GetSectionAsync(string name, CancellationToken cancellationToken = default)
    {
        // get the section
        var section = await _unitOfWork.Sections.GetSectionAsync(name, cancellationToken);
        return await CreateSectionDto(section, cancellationToken);
    }

    private async Task<SectionDto> CreateSectionDto(SectionAggregate section, CancellationToken cancellationToken)
    {
        var variableSets = (await _unitOfWork.VariableSets.FindAsync(new All(), cancellationToken))
            .ToDictionary(
                t => t.Id, 
                t => t);
        
        var composed = VariableSetComposer.Compose(variableSets.Values.Select(t => t.ToVariableSet()));
        var sectionDto = section.ToOutputDto();
        
        // iterate all releases and assign the variables
        foreach (var env in section.Environments)
        {
            var envDto = sectionDto.GetEnvironment(env.EnvironmentName);
            foreach (var release in env.Releases.Where(r => r.VariableSetId != null))
            {
                VariableSetComposed? variableSet = null;
                if (release.VariableSetId == null) continue;
                var name = variableSets[release.VariableSetId].VariableSetName;
                variableSet = composed[name];
                envDto.GetRelease(release.Id.Id).VariableSet = VariableSetComposedDto.FromVariableSetComposed(variableSet);
            }
        }

        return sectionDto;
    }
}