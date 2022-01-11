using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public interface ISourceEvent
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
public record SectionCreatedSourceEvent
(SectionId SectionId, string SectionName, string? Path, ConfigurationSchema? Schema,
    string? TokenSetName) : SourceEventBase;

/// <summary>
/// An environment was added to a configuration section.
/// </summary>
/// <param name="EnvironmentName"></param>
/// <param name="SectionName"></param>
public record EnvironmentAddedToSectionSourceEvent(EnvironmentId EnvironmentId, SectionId SectionId, string Name) : SourceEventBase;

/// <summary>
/// A schema was added to a configuration section.
/// </summary>
/// <param name="SectionId"></param>
/// <param name="Schema"></param>
public record SchemaAddedToSection(SectionId Id, ConfigurationSchema Schema) : SourceEventBase;

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
public record ReleaseCreatedSourceEvent(
    ReleaseId ReleaseId,
    string SectionName,
    string EnvironmentName,
    ConfigurationSchema Schema,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetComposed? Tokens,
    HashSet<string> TokensInUse) : SourceEventBase;

/// <summary>
/// A token set was created.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Tokens"></param>
/// <param name="BaseTokenSetName"></param>
public record TokenSetCreatedSourceEvent(
    string TokenSetName,
    Dictionary<string, JToken> Tokens,
    string BaseTokenSetName) : SourceEventBase;

/// <summary>
/// A release was deployed.
/// </summary>
/// <param name="SectionName"></param>
/// <param name="EnvironmentName"></param>
/// <param name="ReleaseId"></param>
public record ReleaseDeployedSourceEvent(
    DeploymentId DeploymentId,
    DateTime deploymentDate,
    string SectionName,
    string EnvironmentName,
    ReleaseId ReleaseId) : SourceEventBase;

/// <summary>
/// A deployed release is no longer deployed.
/// </summary>
/// <param name="SectionName"></param>
/// <param name="EnvironmentName"></param>
/// <param name="ReleaseId"></param>
/// <param name="Reason"></param>
public record ReleaseRemovedSourceEvent(
    string SectionName,
    string EnvironmentName,
    ReleaseId ReleaseId,
    string Reason) : SourceEventBase;

/// <summary>
/// The value of a token changed.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Key"></param>
/// <param name="Value"></param>
public record TokenValueSetSourceEvent(
    string TokenSetName,
    string Key,
    JToken Value) : SourceEventBase;