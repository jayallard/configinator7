using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model.State;

public record Section
{
    public long Id { get; set; }
    
    public string Name { get; set; }
    public string? Path { get; set; }

    public List<ConfigurationSchema> Schemas { get; } = new();

    public ConfigurationSchema GetSchema(SemanticVersion version) => Schemas.Single(s => s.Version == version);

    public List<ConfigurationEnvironment> Environments = new();

    public string? TokenSetName { get; set; }

    public bool DeployedIsOutOfDate => Environments.Any(e => e.DeployedIsOutOfDate);

    public ConfigurationEnvironment GetEnvironment(string name) =>
        Environments.Single(e => e.EnvironmentId.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

public record ConfigurationSchema(SemanticVersion Version, JsonSchema Schema);

public record ConfigurationEnvironmentId(string Name);

public class ConfigurationEnvironment
{
    public ConfigurationEnvironmentId EnvironmentId { get; set; }

    public List<Release> Releases { get; } = new();

    public bool DeployedIsOutOfDate => Releases.Any(r => r.IsDeployed && r.IsOutOfDate);

    public Release GetRelease(long id) => Releases.Single(r => r.ReleaseId == id);
}