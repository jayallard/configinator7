using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Json;
using NJsonSchema;
using NuGet.Versioning;

namespace ConfiginatorWeb.Queries;

public class SectionDto
{
    public string SectionName { get; set; }
    
    public long SectionId { get; set; }

    public string OrganizationPath { get; set; }

    public List<SectionSchemaDto> Schemas { get; set; }

    public List<SectionEnvironmentDto> Environments { get; set; }

    public SectionEnvironmentDto GetEnvironment(string environmentName) =>
        Environments.Single(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

    public SectionEnvironmentDto GetEnvironment(long environmentId) =>
        Environments.Single(e => e.EnvironmentId == environmentId);

    public SectionSchemaDto GetSchema(SemanticVersion version) =>
        Schemas.Single(s => s.Version == version);
}

public class SectionSchemaDto
{
    public SemanticVersion Version { get; set; }
    public JsonDocument Schema { get; set; }
}

public class SectionEnvironmentDto
{
    public string EnvironmentName { get; set; }
    
    public long EnvironmentId { get; set; }

    public List<SectionReleaseDto> Releases { get; set; }

    public SectionReleaseDto GetRelease(long releaseId) =>
        Releases.Single(r => r.ReleaseId == releaseId);
}

public class SectionReleaseDto
{
    public SectionSchemaDto Schema { get; set; }
    public long ReleaseId { get; set; }

    public DateTime CreateDate { get; set; }

    public bool IsDeployed { get; set; }

    public bool IsOutOfDate { get; set; }

    public VariableSetComposedDto? VariableSet { get; set; }
    
    public JsonDocument ModelValue { get; set; }
    
    public JsonDocument ResolvedValue { get; set; }
    
    public List<SectionDeploymentDto> Deployments { get; set; }
}

public class SectionDeploymentDto
{
    public long DeploymentId { get; set; }
    public DateTime DeploymentDate { get; set; }
    public DateTime? RemovedDate { get; set; }
    public string? RemoveReason { get; set; }
    
    public string? Notes { get; set; }
    public bool IsDeployed => RemovedDate is null;
}