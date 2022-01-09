using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public interface IEvent
{
    DateTime EventDate { get; }
};

/// <summary>
/// A configuration section was created.
/// </summary>
/// <param name="SectionId"></para>
/// <param name="SectionName"></param>
/// <param name="Path"></param>
/// <param name="Schema"></param>
/// <param name="TokenSetName"></param>
public record SectionCreatedEvent
(SectionId SectionId, string SectionName, string? Path, ConfigurationSchema? Schema,
    string? TokenSetName) : EventBase;

/// <summary>
/// An environment was added to a configuration section.
/// </summary>
/// <param name="EnvironmentName"></param>
/// <param name="SectionName"></param>
public record EnvironmentAddedToSectionEvent(EnvironmentId EnvironmentId, SectionId SectionId, string Name) : EventBase;

/// <summary>
/// A schema was added to a configuration section.
/// </summary>
/// <param name="SectionId"></param>
/// <param name="Schema"></param>
public record SchemaAddedToSection(SectionId Id, ConfigurationSchema Schema) : EventBase;

/// <summary>
/// A release was created.
/// </summary>
/// <param name="ReleaseId"></param>
/// <param name="SectionName"></param>
/// <param name="EnvironmentName"></param>
/// <param name="Schema"></param>
/// <param name="ModelValue"></param>
/// <param name="ResolvedValue"></param>
/// <param name="Tokens"></param>
/// <param name="TokensInUse"></param>
public record ReleaseCreatedEvent(
    long ReleaseId,
    string SectionName,
    string EnvironmentName,
    ConfigurationSchema Schema,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetResolved? Tokens,
    HashSet<string> TokensInUse) : EventBase;

/// <summary>
/// A token set was created.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Tokens"></param>
/// <param name="BaseTokenSetName"></param>
public record TokenSetCreatedEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens,
    string BaseTokenSetName) : EventBase;

/// <summary>
/// A release was deployed.
/// </summary>
/// <param name="SectionName"></param>
/// <param name="EnvironmentName"></param>
/// <param name="ReleaseId"></param>
public record ReleaseDeployedEvent(
    string SectionName,
    string EnvironmentName,
    ReleaseId ReleaseId) : EventBase;

/// <summary>
/// A deployed release is no longer deployed.
/// </summary>
/// <param name="SectionName"></param>
/// <param name="EnvironmentName"></param>
/// <param name="ReleaseId"></param>
/// <param name="Reason"></param>
public record ReleaseRemovedEvent(
    string SectionName,
    string EnvironmentName,
    ReleaseId ReleaseId,
    string Reason) : EventBase;

/// <summary>
/// The value of a token changed.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Key"></param>
/// <param name="Value"></param>
public record TokenValueSetEvent(
    string TokenSetName,
    string Key,
    JToken Value) : EventBase;