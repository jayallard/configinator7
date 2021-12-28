using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public interface IEvent
{
};

public record SecretCreatedEvent(string SecretName, string? Path, ConfigurationSchema? Schema, string? TokenSetName) : IEvent;

public record HabitatAddedToSecretEvent(string HabitatName, string SecretName) : IEvent;

public record SchemaAddedToSecret(string SecretName, ConfigurationSchema Schema) : IEvent;

public record ReleaseCreatedEvent(
    string SecretName,
    string HabitatName,
    SemanticVersion Version,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet Tokens) : IEvent;
    
public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens) : IEvent;
