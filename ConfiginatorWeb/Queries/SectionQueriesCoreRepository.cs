using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Configinator.Core.Specifications.Schema;
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
    {
        var items = (await _unitOfWork.Sections.FindAsync(new All(), cancellationToken))
            .Select(s => new SectionListItemDto(s.Id.Id, s.Namespace, s.SectionName,
                s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase)))
            .ToList();
        return items;
    }

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
        var variableSets = await GetVariableSetsByIdAsync(cancellationToken);
        var dto = new SectionDto
        {
            SectionId = section.Id.Id,
            SectionName = section.SectionName,
            Namespace = section.Namespace,
            Environments = new List<SectionEnvironmentDto>(),
            EnvironmentTypes = section.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            Schemas = new List<SchemaDto>()
        };

        var schemas = await _unitOfWork.Schemas.FindAsync(new SchemaSectionIdIs(section.EntityId), cancellationToken);
        foreach (var schema in schemas)
            // todo: inefficient, but convenient
            dto.Schemas.Add(await GetSchemaAsync(schema.EntityId, cancellationToken));

        foreach (var env in section.Environments)
        {
            // environments
            var envDto = new SectionEnvironmentDto
            {
                EnvironmentId = env.Id.Id,
                EnvironmentName = env.EnvironmentName,
                EnvironmentType = env.EnvironmentType,
                Releases = new List<SectionReleaseDto>()
            };
            dto.Environments.Add(envDto);


            // releases
            foreach (var release in env.Releases)
            {
                var variableSetDto = release.VariableSetId is null
                    ? null
                    : variableSets[release.VariableSetId];

                var releaseDto = new SectionReleaseDto
                {
                    CreateDate = release.CreateDate,
                    IsDeployed = release.IsDeployed,
                    ResolvedValue = release.ResolvedValue,
                    IsOutOfDate = release.IsOutOfDate,
                    ModelValue = release.ModelValue,
                    Deployments = new List<SectionDeploymentDto>(),
                    Schema = await GetSchemaAsync(release.SchemaId.Id, cancellationToken),
                    VariableSet = variableSetDto,
                    ReleaseId = release.Id.Id
                };
                envDto.Releases.Add(releaseDto);

                // deployments
                foreach (var deployment in release.Deployments)
                {
                    var deploymentResult = deployment.DeploymentResult == null
                        ? null
                        : new SectionDeploymentResultDto(
                            deployment.DeploymentResult.IsSuccess,
                            deployment.DeploymentResult.Messages.Select(m =>
                                    new DeploymentResultMessage(m.Source, m.Key, m.Severity, m.Message, m.Exception))
                                .ToList());

                    var deploymentDto = new SectionDeploymentDto
                    {
                        Notes = deployment.Notes,
                        DeploymentDate = deployment.DeploymentDate,
                        DeploymentId = deployment.Id.Id,
                        RemovedDate = deployment.RemovedDate,
                        RemoveReason = deployment.RemoveReason,
                        DeploymentResult = deploymentResult
                    };

                    releaseDto.Deployments.Add(deploymentDto);
                }
            }
        }

        return dto;
    }

    private async Task<IDictionary<VariableSetId, VariableSetComposedDto>> GetVariableSetsByIdAsync(
        CancellationToken cancellationToken)
    {
        // this is ugly.
        // we have variable set aggregates.
        // need to convert them to variable set dtos, retrievable by id.
        // we lose the ID when we compose.
        // so, need to convert, then map from the converted name back to the id.
        // the separation of variable set from variable set aggregate might be more trouble than
        // it's worth. i'll revisit.
        var variableSetAggregates = (await _unitOfWork.VariableSets.FindAsync(new All(), cancellationToken)).ToList();

        // convert aggregates to variable sets.
        var variableSets = variableSetAggregates.Select(t => t.ToVariableSet()).ToList();
        var byName = VariableSetComposer
            .Compose(variableSets)
            .ToDictionary(vs => vs.VariableSetName, vs => vs, StringComparer.OrdinalIgnoreCase);
        var byId = variableSetAggregates.ToDictionary(a => a.Id,
            a => VariableSetComposedDto.FromVariableSetComposed(byName[a.VariableSetName]));
        return byId;
    }

    private async Task<SchemaDto> GetSchemaAsync(
        long schemaId,
        CancellationToken cancellationToken)
    {
        var s = await _unitOfWork.Schemas.GetAsync(new SchemaId(schemaId), cancellationToken);
        return new SchemaDto
        {
            Schema = s.Schema,
            EnvironmentTypes = s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            SchemaName = s.SchemaName.ToOutputDto(),
            SectionId = s.SectionId?.Id,
            SchemaId = s.Id.Id
        };
    }
}