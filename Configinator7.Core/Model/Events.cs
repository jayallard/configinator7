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

public record SectionCreatedEvent(string SectionName, string? Path, ConfigurationSchema? Schema, string? TokenSetName) : EventBase;

public record EnvironmentAddedToSectionEvent(string EnvironmentName, string SectionName) : EventBase;

public record SchemaAddedToSection(string SectionName, ConfigurationSchema Schema) : EventBase;

public record ReleaseCreatedEvent(
    long ReleaseId,
    string SectionName,
    string EnvironmentName,
    ConfigurationSchema Schema,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetResolved? Tokens) : EventBase;

public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens,
    string BaseTokenSetName) : EventBase;

public record ReleaseDeployed(
    string SectionName,
    string EnvironmentName,
    long ReleaseId): EventBase;

public record ReleaseUndeployed(
    string SectionName,
    string EnvironmentName,
    long ReleaseId,
    string Reason) : EventBase;
    