using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.Json;
using NJsonSchema;
using NuGet.Versioning;

namespace ConfiginatorWeb.Queries;

public class SectionView
{
    public string SectionName { get; set; }

    public string Path { get; set; }

    public List<SectionSchemaView> Schemas { get; set; }

    public List<SectionEnvironmentView> Environments { get; set; }

    public SectionEnvironmentView GetEnvironment(string environmentName) =>
        Environments.Single(e => e.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

    public SectionSchemaView GetSchema(SemanticVersion version) =>
        Schemas.Single(s => s.Version == version);
}

public class SectionSchemaView
{
    public SemanticVersion Version { get; set; }
    public JsonSchema Schema { get; set; }
}

public class SectionEnvironmentView
{
    public string EnvironmentName { get; set; }

    public List<SectionReleaseView> Releases { get; set; }

    public SectionReleaseView GetRelease(long releaseId) =>
        Releases.Single(r => r.ReleaseId == releaseId);
}

public class SectionReleaseView
{
    public SectionSchemaView Schema { get; set; }
    public long ReleaseId { get; set; }

    public DateTime CreateDate { get; set; }

    public bool IsDeployed { get; set; }

    public bool IsOutOfDate { get; set; }

    public TokenSetComposed? TokenSet { get; set; }
    
    public JsonDocument ModelValue { get; set; }
    
    public JsonDocument ResolvedValue { get; set; }
    
    public List<SectionDeploymentHistoryView> DeploymentHistory { get; set; }
}

public class SectionDeploymentHistoryView
{
    public long DeploymentHistoryId { get; set; }
    public DateTime DeploymentDate { get; set; }
    public DeploymentStatus DeploymentStatus { get; set; }
    public string? Reason { get; set; }
    public bool IsDeployed { get; set; }
}