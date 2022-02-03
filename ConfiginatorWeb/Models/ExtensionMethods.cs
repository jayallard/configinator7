using Allard.Configinator.Core.Model;
using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models;

public static class ExtensionMethods
{
    public static SectionDto ToOutputDto(this SectionAggregate section) => new()
    {
        SectionName = section.SectionName,
        Path = section.Path,
        Environments = section.Environments.Select(e => new SectionEnvironmentDto
        {
            EnvironmentName = e.EnvironmentName,
            Releases = e.Releases.Select(r => new SectionReleaseDto
            {
                Schema = new SectionSchemaDto
                {
                    Schema = r.Schema.Schema,
                    Version = r.Schema.Version
                },
                CreateDate = r.CreateDate,
                IsDeployed = r.IsDeployed,
                IsOutOfDate = r.IsOutOfDate,
                ReleaseId = r.Id.Id,
                TokenSet = r.TokenSet,
                ModelValue = r.ModelValue,
                ResolvedValue = r.ResolvedValue,
                Deployments = r.Deployments.Select(d => new SectionDeploymentDto
                {
                    DeploymentId = d.Id.Id,
                    DeploymentDate = d.DeploymentDate,
                    RemovedDate = d.RemovedDate,
                    RemoveReason = d.RemoveReason
                }).ToList()
            }).ToList()
        }).ToList(),
        Schemas = section.Schemas.Select(s => new SectionSchemaDto
        {
            Schema = s.Schema,
            Version = s.Version
        }).ToList()
    };
}