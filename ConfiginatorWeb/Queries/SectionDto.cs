using System.Text.Json;
using Allard.Configinator.Core.Model;
using NuGet.Versioning;

namespace ConfiginatorWeb.Queries;

public class SectionDto
{
    public string SectionName { get; set; }

    public long SectionId { get; set; }

    public List<SectionSchemaDto> Schemas { get; set; }

    public List<SectionEnvironmentDto> Environments { get; set; }

    public SectionEnvironmentDto GetEnvironment(string environmentName)
    {
        return Environments.Single(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
    }

    public SectionEnvironmentDto GetEnvironment(long environmentId)
    {
        return Environments.Single(e => e.EnvironmentId == environmentId);
    }

    public SectionSchemaDto GetSchema(string name)
    {
        return Schemas.Single(s => s.SchemaName.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

public class SectionSchemaDto
{
    public long SchemaId { get; set; }
    public long SectionId { get; set; }
    public SchemaNameDto SchemaName { get; set; }
    public JsonDocument Schema { get; set; }
    public ISet<string> EnvironmentTypes { get; set; }
}

public record SchemaNameDto(string Name, SemanticVersion Version, string FullName)
{
}

public class SectionEnvironmentDto
{
    public string EnvironmentName { get; set; }

    public long EnvironmentId { get; set; }

    public string EnvironmentType { get; set; }

    public List<SectionReleaseDto> Releases { get; set; }

    public SectionReleaseDto GetRelease(long releaseId)
    {
        return Releases.Single(r => r.ReleaseId == releaseId);
    }
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

    public SectionDeploymentDto GetDeployment(long deploymentId)
    {
        return Deployments.Single(d => d.DeploymentId == deploymentId);
    }
}

public class SectionDeploymentDto
{
    public long DeploymentId { get; set; }
    public DateTime DeploymentDate { get; set; }
    public DateTime? RemovedDate { get; set; }
    public string? RemoveReason { get; set; }
    public string? Notes { get; set; }
    public bool IsDeployed => RemovedDate is null;

    public SectionDeploymentResultDto? DeploymentResult { get; set; }
}

public record SectionDeploymentResultDto(
    bool IsSuccess,
    List<DeploymentResultMessage> Messages);

public record SectionDeploymentResultMessageDto(
    string Source,
    string Key,
    LogLevel Severity,
    string Message,
    Exception? ex = null);