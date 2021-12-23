using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;

namespace Configinator7.Core.Model;

public record TokenSetId(string Name, long Version);
public class TokenSet
{
    public TokenSetId Id { get; set; }
    public Dictionary<string, JToken> Tokens { get; set; }
}

public record SecretId(string Name, long Version);
public record Secret
{
    public SecretId Id { get; set; }
    public string Path { get; set; }
    public JsonSchema Schema { get; set; }

    public List<Habitat> Habitats = new();
}

public record HabitatId(string Name, long Version);
public class Habitat
{
    public HabitatId HabitatId { get; set; }
    public JObject Value { get; set; }
}

public record ResolvedId(HabitatId HabitatId, long Version);
public class Resolved
{
    public JObject ResolvedValue { get; set; }
    public bool IsValid => Failures == null || Failures.Count == 0;
    public List<ValidationError> Failures { get; set; }
}