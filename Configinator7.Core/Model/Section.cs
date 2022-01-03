using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public record SectionId(string Name);

public record Section
{
    public SectionId Id { get; set; }
    public string Path { get; set; }

    public List<ConfigurationSchema> Schemas { get; } = new();

    public List<ConfigurationEnvironment> Environments = new();
    
    public string TokenSetName { get; set; }
}

public record ConfigurationSchema(SemanticVersion Version, JsonSchema Schema);

public record ConfigurationEnvironmentId(string Name);

public class ConfigurationEnvironment
{
    public ConfigurationEnvironmentId EnvironmentId { get; set; }
    
    public List<Release> Releases { get; } = new();
}

public record Release(
    long ReleaseId,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetResolved? TokenSet,
    ConfigurationSchema Schema,
    DateTime CreateDate)
{
    public List<Deployment> Deployments { get; } = new();
    public bool IsDeployed { get; set; }
}

public record Deployment(DateTime DeploymentDate, DeploymentAction Action, string Reason)
{
    public bool IsDeployed { get; set; }
}

public enum DeploymentAction
{
    Deployed, Removed
}

