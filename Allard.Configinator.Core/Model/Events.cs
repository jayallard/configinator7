using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public interface IEvent
{
    DateTime EventDate { get; }
};

public record EventBase : IEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}

public record SectionCreatedEvent
    (string SectionName, string? Path, ConfigurationSchema? Schema, string? TokenSetName) : EventBase;

public record EnvironmentAddedToSectionEvent(string EnvironmentName, string SectionName) : EventBase;

public record SchemaAddedToSection(string SectionName, ConfigurationSchema Schema) : EventBase;

public record ReleaseCreatedEvent(
    long ReleaseId,
    string SectionName,
    string EnvironmentName,
    ConfigurationSchema Schema,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetResolved? Tokens,
    HashSet<string> TokensInUse) : EventBase;

public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens,
    string BaseTokenSetName) : EventBase;

public record ReleaseDeployed(
    string SectionName,
    string EnvironmentName,
    long ReleaseId) : EventBase;

public record ReleaseRemoved(
    string SectionName,
    string EnvironmentName,
    long ReleaseId,
    string Reason) : EventBase;

public record TokenValueSet(
    string TokenSetName,
    string Key,
    JToken Value) : EventBase;