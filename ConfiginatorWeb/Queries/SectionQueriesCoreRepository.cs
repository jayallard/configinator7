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
    {
        return (await _unitOfWork.Sections.FindAsync(new AllSections(), cancellationToken))
            .Select(s => new SectionListItemDto(s.Id.Id, s.SectionName,
                s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase)))
            .ToList();
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
            Environments = new List<SectionEnvironmentDto>(),
            Schemas = new List<SectionSchemaDto>()
        };

        foreach (var schema in section.Schemas)
        {
            var schemaDto = await GetSchemaAsync(schema.Id, cancellationToken);
            dto.Schemas.Add(schemaDto);
        }

        foreach (var env in section.Environments)
        {
            // environments
            var envDto = new SectionEnvironmentDto
            {
                EnvironmentId = env.Id.Id,
                EnvironmentName = env.EnvironmentName,
                EnvironmentType = env.EnviromentType,
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

    private async Task<SectionSchemaDto> GetSchemaAsync(
        long schemaId,
        CancellationToken cancellationToken)
    {
        var s = await _unitOfWork.Schemas.GetAsync(new SchemaId(schemaId), cancellationToken);
        return new SectionSchemaDto
        {
            Schema = s.Schema,
            EnvironmentTypes = s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
            SchemaName = s.SchemaName.ToOutputDto(),
            SectionId = s.SectionId!.Id,
            SchemaId = s.Id.Id
        };
    }
}