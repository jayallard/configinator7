using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public record SecretId(string Name);

public record Secret
{
    public SecretId Id { get; set; }
    public string Path { get; set; }

    public List<ConfigurationSchema> Schemas { get; } = new();

    public List<Habitat> Habitats = new();
    
    public string TokenSetName { get; set; }
}

public record ConfigurationSchema(SemanticVersion Version, JsonSchema Schema);

public record HabitatId(string Name);

public class Habitat
{
    public HabitatId HabitatId { get; set; }
    
    public List<Release> Releases { get; } = new();
}

public record Release(
    long ReleaseId,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet TokenSet,
    SemanticVersion SchemaVersion,
    DateTime CreateDate,
    ICollection<ValidationError>? Errors
)
{
    public List<Deployment> Deployments { get; } = new();
    public bool IsDeployed { get; set; }
}

public record Deployment(DateTime DeploymentDate, DeploymentAction Action, string Reason);

public enum DeploymentAction
{
    Set, Removed
}

public record TokenSet;