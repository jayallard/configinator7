using System.Collections.Immutable;
using System.Text.Json;
using Allard.Json;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    internal List<DeploymentEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentEntity> Deployments => InternalDeployments.AsReadOnly();

    /// <summary>
    /// Gets the configuration value with optional tokens.
    /// </summary>
    public JsonDocument ModelValue { get; }

    /// <summary>
    /// Gets the resolved configuration value. This is the ModelValue
    /// with the token replacements completed.
    /// </summary>
    public JsonDocument ResolvedValue { get; }

    /// <summary>
    /// Gets or sets the token set used for this release.
    /// This is the TokenSet as-of the time that the release was created.
    /// The copy is immutable. Changes made to the TokenSet post-release creation
    /// are not represented.
    /// </summary>
    public TokenSetComposed? TokenSet { get; }

    /// <summary>
    /// The schema used for this release.
    /// </summary>
    public SchemaEntity Schema { get; }

    /// <summary>
    /// Gets the date that the release was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    /// Gets a value indicating whether this release is currently deployed.
    /// </summary>
    public bool IsDeployed { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this release is out of date.
    /// A release falls out of date if, for example, it uses a token
    /// and the value of the token has changed.
    /// </summary>
    public bool IsOutOfDate { get; private set; }

    public ImmutableHashSet<string> TokensInUse { get; }

    /// <summary>
    /// Set the value of the IsDeployed property.
    /// </summary>
    /// <param name="isDeployed"></param>
    internal void SetDeployed(bool isDeployed) => IsDeployed = isDeployed;

    internal void SetOutOfDate(bool isOutOfDate) => IsOutOfDate = isOutOfDate;

    /// <summary>
    /// Initializes a new instance of the ReleaseEntity class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="schema"></param>
    /// <param name="modelValue"></param>
    /// <param name="resolvedValue"></param>
    /// <param name="tokenSet"></param>
    internal ReleaseEntity(
        ReleaseId id,
        SchemaEntity schema,
        JsonDocument modelValue,
        JsonDocument resolvedValue,
        TokenSetComposed? tokenSet) : base(id)
    {
        Schema = schema;
        ModelValue = modelValue;
        ResolvedValue = resolvedValue;
        TokenSet = tokenSet;
        CreateDate = DateTime.Now;
        TokensInUse = JsonUtility
            .GetTokens(resolvedValue.ToJsonNetJson())
            .Select(t => t.TokenName)
            .ToImmutableHashSet();
    }
}