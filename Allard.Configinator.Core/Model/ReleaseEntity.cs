using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Allard.Json;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    protected internal readonly List<DeploymentEntity> _deployments = new();

    public ReleaseEntity()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the ReleaseEntity class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="schemaId"></param>
    /// <param name="modelValue"></param>
    /// <param name="resolvedValue"></param>
    /// <param name="variableSetIdId"></param>
    public ReleaseEntity(
        ReleaseId id,
        SchemaId schemaId,
        JsonDocument modelValue,
        JsonDocument resolvedValue,
        VariableSetId? variableSetIdId,
        DateTime createDate)
    {
        Id = id;
        SchemaId = schemaId;
        ModelValue = modelValue;
        ResolvedValue = resolvedValue;
        VariableSetId = variableSetIdId;
        CreateDate = createDate;
        VariablesInUse = JsonUtility
            .GetVariables(modelValue.ToJsonNetJson())
            .Select(t => t.VariableName)
            .ToImmutableHashSet();
    }

    [JsonInclude]
    public IEnumerable<DeploymentEntity> Deployments
    {
        get => _deployments.AsReadOnly();
        private init => _deployments = value.ToList();
    }

    /// <summary>
    ///     Gets the configuration value with optional variables.
    /// </summary>
    [JsonInclude]
    public JsonDocument ModelValue { get; private init; }

    /// <summary>
    ///     Gets the resolved configuration value. This is the ModelValue
    ///     with the variable replacements completed.
    /// </summary>
    [JsonInclude]
    public JsonDocument ResolvedValue { get; private init; }

    /// <summary>
    ///     Gets or sets the variable set used for this release.
    ///     This is the VariableSet as-of the time that the release was created.
    ///     The copy is immutable. Changes made to the VariableSet post-release creation
    ///     are not represented.
    /// </summary>
    // BUG: variable sets are mutable. this needs a copy, or variable sets need to be versioned.
    // versioned variable sets would be a lot more complicated
    [JsonInclude]
    public VariableSetId? VariableSetId { get; private init; }

    /// <summary>
    ///     The schema used for this release.
    /// </summary>
    [JsonInclude]
    public SchemaId SchemaId { get; private init; }

    /// <summary>
    ///     Gets the date that the release was created.
    /// </summary>
    [JsonInclude]
    public DateTime CreateDate { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether this release is currently deployed.
    /// </summary>
    public bool IsDeployed { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether this release is out of date.
    ///     A release falls out of date if, for example, it uses a variable
    ///     and the value of the variable has changed.
    /// </summary>
    public bool IsOutOfDate { get; private set; }

    [JsonInclude]
    public ImmutableHashSet<string> VariablesInUse { get; private init; }

    /// <summary>
    ///     Set the value of the IsDeployed property.
    /// </summary>
    /// <param name="isDeployed"></param>
    internal void SetDeployed(bool isDeployed)
    {
        IsDeployed = isDeployed;
    }

    internal void SetOutOfDate(bool isOutOfDate)
    {
        IsOutOfDate = isOutOfDate;
    }
}