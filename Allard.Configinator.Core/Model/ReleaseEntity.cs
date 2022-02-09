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
    // BUG: token sets are mutable. this needs a copy, or token sets need to be versioned.
    // versioned token sets would be a lot more complicated
    public TokenSetId? TokenSetId { get; }

    /// <summary>
    /// The schema used for this release.
    /// </summary>
    public SectionSchemaEntity SectionSchema { get; }

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
    /// <param name="sectionSchema"></param>
    /// <param name="modelValue"></param>
    /// <param name="resolvedValue"></param>
    /// <param name="tokenSetIdId"></param>
    internal ReleaseEntity(
        ReleaseId id,
        SectionSchemaEntity sectionSchema,
        JsonDocument modelValue,
        JsonDocument resolvedValue,
        TokenSetId? tokenSetIdId)
    {
        Id = id;
        SectionSchema = sectionSchema;
        ModelValue = modelValue;
        ResolvedValue = resolvedValue;
        TokenSetId = tokenSetIdId;
        CreateDate = DateTime.Now;
        TokensInUse = JsonUtility
            .GetTokens(modelValue.ToJsonNetJson())
            .Select(t => t.TokenName)
            .ToImmutableHashSet();
    }
}