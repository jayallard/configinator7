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

    public List<HabitatSchema> Schemas { get; } = new();

}

public class HabitatSchema
{
    public ConfigurationSchema Schema { get; set; }
    
    public JObject ModelValue { get; set; }

    public List<ResolvedConfigurationValue> Resolved { get; } = new();

}

public record ResolvedConfigurationValue(
    JObject ModelValue, 
    JObject ResolvedValue, 
    TokenSet TokenSet, 
    ICollection<ValidationError> Errors);

public record TokenSet;