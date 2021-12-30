using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public interface IEvent
{
};

public record EventBase : IEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}

public record SecretCreatedEvent(string SecretName, string? Path, ConfigurationSchema? Schema, string? TokenSetName) : EventBase;

public record HabitatAddedToSecretEvent(string HabitatName, string SecretName) : EventBase;

public record SchemaAddedToSecret(string SecretName, ConfigurationSchema Schema) : EventBase;

public record ReleaseCreatedEvent(
    long ReleaseId,
    string SecretName,
    string HabitatName,
    SemanticVersion Version,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet Tokens) : EventBase;

public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens) : EventBase;

public record ReleaseDeployed(
    string SecretName,
    string HabitatName,
    long ReleaseId): EventBase;

public record ReleaseUndeployed(
    string SecretName,
    string HabitatName,
    long ReleaseId,
    string Reason) : EventBase;
    