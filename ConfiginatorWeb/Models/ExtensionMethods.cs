using Allard.Configinator.Core.Model;
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
            Version = release.SectionSchema.Version
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
            RemoveReason = d.RemoveReason
        }).ToList()
    };
    
    public static SectionDto ToOutputDto(this SectionAggregate section) => new()
    {
        SectionName = section.SectionName,
        Path = section.Path,
        Environments = section.Environments.Select(e => new SectionEnvironmentDto
        {
            EnvironmentName = e.EnvironmentName,
            Releases = e.Releases.ToOutputDto().ToList()
        }).ToList(),
        Schemas = section.Schemas.Select(s => new SectionSchemaDto
        {
            Schema = s.Schema,
            Version = s.Version
        }).ToList()
    };
    
}