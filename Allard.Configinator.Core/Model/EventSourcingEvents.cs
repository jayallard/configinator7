using System.Text.Json;
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
public record SectionCreatedSourceEvent(
    SectionId SectionId,
    string SectionName,
    string? Path,
    SchemaEntity? Schema,
    string? TokenSetName) : SourceEventBase;

/// <summary>
/// An environment was added to a configuration section.
/// </summary>
/// <param name="EnvironmentId"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentName"></param>
public record EnvironmentAddedToSectionSourceEvent(
    EnvironmentId EnvironmentId, 
    SectionId SectionId,
    string EnvironmentName) : SourceEventBase;

/// <summary>
/// A schema was added to a configuration section.
/// </summary>
/// <param name="SectionId"></param>
/// <param name="Schema"></param>
public record SchemaAddedToSection(SectionId SectionId, SchemaEntity Schema) : SourceEventBase;

/// <summary>
/// A release was created.
/// </summary>
/// <param name="ReleaseId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="SectionId"></param>
/// <param name="SchemaId"></param>
/// <param name="ModelValue"></param>
/// <param name="ResolvedValue"></param>
/// <param name="Tokens"></param>
/// <param name="TokensInUse"></param>
public record ReleaseCreatedSourceEvent(
    ReleaseId ReleaseId,
    EnvironmentId EnvironmentId,
    SectionId SectionId,
    SchemaId SchemaId,
    JsonDocument ModelValue,
    JsonDocument ResolvedValue,
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
/// <param name="DeploymentHistoryId"></param>
/// <param name="deploymentDate"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="ReleaseId"></param>
public record ReleaseDeployedSourceEvent(
    DeploymentHistoryId DeploymentHistoryId,
    DateTime deploymentDate,
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId) : SourceEventBase;

/// <summary>
/// A deployed release is no longer deployed.
/// </summary>
/// <param name="DeploymentHistoryId"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="ReleaseId"></param>
/// <param name="Reason"></param>
public record DeploymentRemovedSourceEvent(
    DeploymentHistoryId DeploymentHistoryId,
    SectionId SectionId,
    EnvironmentId EnvironmentId,
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