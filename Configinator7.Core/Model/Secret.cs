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
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet TokenSet,
    SemanticVersion SchemaVersion,
    ICollection<ValidationError>? Errors
);

public record TokenSet;