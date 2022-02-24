using System.Text.Json;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core;

/// <summary>
///     A configuration section was created.
/// </summary>
/// <param name="SectionId"></para>
///     <param name="SectionName"></param>
public record SectionCreatedEvent(
    SectionId SectionId,
    string Namespace,
    string SectionName,
    string InitialEnvironmentType) : DomainEventBase;

/// <summary>
///     An environment was added to a configuration section.
/// </summary>
/// <param name="EnvironmentId"></param>
/// <param name="SectionId"></param>
/// <param name="EnvironmentName"></param>
public record EnvironmentCreatedEvent(
    EnvironmentId EnvironmentId,
    SectionId SectionId,
    string EnvironmentType,
    string EnvironmentName) : DomainEventBase;

/// <summary>
///     A schema was added to a configuration section.
/// </summary>
/// <param name="SectionId"></param>
/// <param name="Schema"></param>
public record SchemaAddedToSectionEvent(
    SectionId SectionId,
    SchemaId SchemaId) : DomainEventBase;

/// <summary>
///     A release was created.
/// </summary>
/// <param name="ReleaseId"></param>
/// <param name="EnvironmentId"></param>
/// <param name="SectionId"></param>
/// <param name="SchemaId"></param>
/// <param name="ModelValue"></param>
/// <param name="ResolvedValue"></param>
/// <param name="VariablesInUse"></param>
public record ReleaseCreatedEvent(
    ReleaseId ReleaseId,
    EnvironmentId EnvironmentId,
    SectionId SectionId,
    SchemaId SchemaId,
    VariableSetId? VariableSetId,
    JsonDocument ModelValue,
    JsonDocument ResolvedValue,
    HashSet<string> VariablesInUse) : DomainEventBase;

/// <summary>
///     A variable set was created.
/// </summary>
/// <param name="VariableSetName"></param>
/// <param name="Variables"></param>
/// <param name="BaseVariableSetName"></param>
public record VariableSetCreatedEvent(
    VariableSetId VariableSetId,
    VariableSetId? BaseVariableSetId,

    // TODO: transitional. delete this. (it'll break stuff)
    string? BaseVariableSetName,
    string Namespace,
    string VariableSetName,
    string EnvironmentType) : DomainEventBase;

public record VariableSetOverrideCreatedEvent(
    VariableSetId VariableSetId,
    VariableSetId BaseVariableSetId) : DomainEventBase;

/// <summary>
///     A release was deployed.
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
    ReleaseId ReleaseId,
    DeploymentResult DeploymentResult,
    string? Notes) : DomainEventBase;

/// <summary>
///     A deployed release is no longer deployed.
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

// todo: SET is too broad... create a CREATED event.

/// <summary>
///     The value of a variable changed.
/// </summary>
/// <param name="VariableSetName"></param>
/// <param name="VariableName"></param>
/// <param name="Value"></param>
public record VariableValueSetEvent(
    string VariableSetName,
    string VariableName,
    JToken Value) : DomainEventBase;

public record VariableValueCreatedEvent(
    string VariableSetName,
    string VariableName,
    JToken Value) : DomainEventBase;

public record ReleaseValueBecameCurrent(
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId) : DomainEventBase;

public record ReleaseValueBecameOld(
    SectionId SectionId,
    EnvironmentId EnvironmentId,
    ReleaseId ReleaseId) : DomainEventBase;

public record SchemaCreatedEvent(
    SchemaId SchemaId,
    SectionId? SectionId,
    string Namespace,
    SchemaName Name,
    string? Description,
    string EnvironmentType,
    JsonDocument Schema) : DomainEventBase;

public record SchemaPromotedEvent(
    SchemaId SchemaId,
    string ToEnvironmentType) : DomainEventBase;