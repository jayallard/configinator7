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

public record ConfigurationSectionCreatedEvent(string ConfigurationSectionName, string? Path, ConfigurationSchema? Schema, string? TokenSetName) : EventBase;

public record HabitatAddedToConfigurationSectionEvent(string HabitatName, string ConfigurationSectionName) : EventBase;

public record SchemaAddedToConfigurationSection(string ConfigurationSectionName, ConfigurationSchema Schema) : EventBase;

public record ReleaseCreatedEvent(
    long ReleaseId,
    string ConfigurationSectionName,
    string HabitatName,
    SemanticVersion Version,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet Tokens) : EventBase;

public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens) : EventBase;

public record ReleaseDeployed(
    string ConfigurationSectionName,
    string HabitatName,
    long ReleaseId): EventBase;

public record ReleaseUndeployed(
    string ConfigurationSectionName,
    string HabitatName,
    long ReleaseId,
    string Reason) : EventBase;
    