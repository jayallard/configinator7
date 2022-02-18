using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using Allard.Json;
using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models;

public static class ExtensionMethods
{
    public static IEnumerable<SectionReleaseDto> ToOutputDto(this IEnumerable<ReleaseEntity> releases) =>
        releases.Select(ToOutputDto);

    public static SectionReleaseDto ToOutputDto(this ReleaseEntity release) => new()
    {
        Schema = new SectionSchemaDto
        {
            Schema = release.SectionSchema.Schema,
            Name = release.SectionSchema.SchemaName.ToOutputDto(),
            EnvironmentTypes = release.SectionSchema.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase)
        },
        CreateDate = release.CreateDate,
        IsDeployed = release.IsDeployed,
        IsOutOfDate = release.IsOutOfDate,
        ReleaseId = release.Id.Id,
        ModelValue = release.ModelValue,
        ResolvedValue = release.ResolvedValue,
        Deployments = release.Deployments.Select(d => new SectionDeploymentDto
        {
            DeploymentId = d.Id.Id,
            DeploymentDate = d.DeploymentDate,
            RemovedDate = d.RemovedDate,
            RemoveReason = d.RemoveReason,
            Notes = d.Notes,
            DeploymentResult = 
                d.DeploymentResult == null 
                ? null 
                : new SectionDeploymentResultDto(
                    d.DeploymentResult.IsSuccess, 
                    d.DeploymentResult.Messages.Select(m => new DeploymentResultMessage(
                        m.Source, m.Key, m.Severity, m.Message, m.Exception)
                        ).ToList())
        }).ToList()
    };

    public static SectionDto ToOutputDto(this SectionAggregate section) => new()
    {
        SectionName = section.SectionName,
        SectionId = section.EntityId,
        Environments = section.Environments.Select(e => new SectionEnvironmentDto
        {
            EnvironmentName = e.EnvironmentName,
            EnvironmentId = e.EntityId,
            Releases = e.Releases.ToOutputDto().ToList(),
        }).ToList(),
        Schemas = section.Schemas.Select(s => new SectionSchemaDto
        {
            Schema = s.Schema,
            Name = s.SchemaName.ToOutputDto(),
            EnvironmentTypes = s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase)
        }).ToList()
    };

    public static SchemaInfoDto ToOutputDto(this SchemaInfo schema) =>
        new()
        {
            Root = schema.Root.ToOutputDto(),
            References = schema.References.Select(ToOutputDto).ToList(),
        };

    public static SchemaDetailDto ToOutputDto(this SchemaDetail detail) =>
        new()
        {
            ReferencedBy = detail.ReferencedBy.ToList(),
            RefersTo = detail.RefersTo.ToList(),
            ResolvedSchema = detail.ResolvedSchema,
            SchemaSource = detail.SchemaSource,
            Name = detail.Name.FullName
        };

    public static SchemaNameDto ToOutputDto(this SchemaName name) =>
        new SchemaNameDto(name.Name, name.Version, name.FullName);
}