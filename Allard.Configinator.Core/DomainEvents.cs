using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

/// <summary>
/// A configuration section was created.
/// </summary>
/// <param name="SectionId"></para>
/// <param name="SectionName"></param>
/// <param name="Path"></param>
/// <param name="Schema"></param>
public record SectionCreatedEvent(
    SectionId SectionId,
    string SectionName,
    string? Path,
    SchemaEntity? Schema) : DomainEventBase;

/// <summary>
/// An environment was added to a configuration section.
/// </summary>
/// <param name="EnvironmentId"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentName"></param>
public record EnvironmentCreatedEvent(
    EnvironmentId EnvironmentId, 
    SectionId SectionId,
    string EnvironmentName) : DomainEventBase;

/// <summary>
/// A schema was added to a configuration section.
/// </summary>
/// <param name="SectionId"></param>
/// <param name="Schema"></param>
public record SchemaAddedToSectionEvent(
    SectionId SectionId,
    SchemaId SchemaId, 
    SemanticVersion SchemaVersion, 
    JsonSchema Schema) : DomainEventBase;

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
public record ReleaseCreatedEvent(
    ReleaseId ReleaseId,
    EnvironmentId EnvironmentId,
    SectionId SectionId,
    SchemaId SchemaId,
    JsonDocument ModelValue,
    JsonDocument ResolvedValue,
    TokenSetComposed? Tokens,
    HashSet<string> TokensInUse) : DomainEventBase;

/// <summary>
/// A token set was created.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Tokens"></param>
/// <param name="BaseTokenSetName"></param>
public record TokenSetCreatedEvent(
    TokenSetId TokenSetId,
    string TokenSetName,
    Dictionary<string, JToken>? Tokens,
    string? BaseTokenSetName) : DomainEventBase;

/// <summary>
/// A release was deployed.
/// </summary>
/// <param name="DeploymentId"></param>
/// <param name="DeploymentDate"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="ReleaseId"></param>
public record ReleaseDeployedEvent(
    DeploymentId DeploymentId,
    DateTime DeploymentDate,
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId) : DomainEventBase;

/// <summary>
/// A deployed release is no longer deployed.
/// </summary>
/// <param name="DeploymentId"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="ReleaseId"></param>
/// <param name="RemoveReason"></param>
public record DeploymentRemovedEvent(
    DeploymentId DeploymentId,
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId,
    string RemoveReason) : DomainEventBase;

/// <summary>
/// The value of a token changed.
/// </summary>
/// <param name="TokenSetName"></param>
/// <param name="Key"></param>
/// <param name="Value"></param>
public record TokenValueSetEvent(
    string TokenSetName,
    string Key,
    JToken Value) : DomainEventBase;
    
public record ReleaseFellOutOfDate(
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId) : DomainEventBase;