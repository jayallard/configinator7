using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class NamespaceQueriesCoreRepository : INamespaceQueries
{
    private readonly IUnitOfWork _unitOfWork;

    public NamespaceQueriesCoreRepository(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NamespaceDto>> GetNamespaces()
    {
        var ns = await _unitOfWork.Namespaces.FindAsync(new All());

        var schemaIds = ns.SelectMany(n => n.Schemas.Select(s => s.Id));
        var schemas = _unitOfWork.Schemas.FindAsync(new IdIsIn(schemaIds));

        var variableSetIds = ns.SelectMany(n => n.VariableSets.Select(v => v.Id));
        var variableSets = _unitOfWork.VariableSets.FindAsync(new IdIsIn(variableSetIds));

        var sectionIds = ns.SelectMany(n => n.Sections.Select(s => s.Id));
        var sections = _unitOfWork.Sections.FindAsync(new IdIsIn(sectionIds));

        var sectionDtos = (await sections)
            .Select(s => new NamespaceSectionDto(
                s.EntityId,
                s.SectionName,
                s.Environments.Select(e =>
                    new NamespaceSectionEnvironmentDto(
                        e.EnvironmentName,
                        e.Releases.Any(r => r.IsDeployed && r.IsOutOfDate),
                        e.Releases.Any(r => r.IsDeployed))).ToList()))
            .ToDictionary(s => s.SectionId);

        var schemaDtos = (await schemas)
            .Select(s =>
            {
                if (s.SectionId == null)
                    return new NamespaceSchemaDto(s.EntityId, s.SchemaName.ToOutputDto(), null, null,
                        s.EnvironmentTypes.ToList());

                var sectionId = s.SectionId.Id;
                return new NamespaceSchemaDto(s.EntityId, s.SchemaName.ToOutputDto(), sectionId,
                    sectionDtos[sectionId].SectionName, s.EnvironmentTypes.ToList());
            })
            .ToDictionary(s => s.SchemaId);

        var variableSetDtos = (await variableSets)
            .Select(vs => new NamespaceVariableSetDto(vs.EntityId, vs.VariableSetName, vs.EnvironmentType))
            .ToDictionary(vs => vs.VariableSetId);


        var result = ns.Select(n =>
            new NamespaceDto
            {
                NamespaceId = n.EntityId,
                NamespaceName = n.Namespace,
                Schemas = n.Schemas.Select(s => schemaDtos[s.Id]).ToList(),
                Sections = n.Sections.Select(s => sectionDtos[s.Id]).ToList(),
                VariableSets = n.VariableSets.Select(v => variableSetDtos[v.Id]).ToList()
            });
        return result.ToList();
    }
}