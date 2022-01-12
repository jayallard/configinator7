using System.Text.Json;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentId>
{
    internal List<ReleaseEntity> InternalReleases { get; } = new();
    public IEnumerable<ReleaseEntity> Releases => InternalReleases.AsReadOnly();


    public EnvironmentEntity(SectionEntity parent, EnvironmentId id, string environmentName) : base(id)
    {
        EnvironmentName = environmentName;
        ParentSection = Guards.NotDefault(parent, nameof(parent));
    }

    public ReleaseEntity GetRelease(ReleaseId releaseId) => InternalReleases.GetRelease(releaseId);

    public async Task<ReleaseEntity> CreateReleaseAsync(ReleaseId releaseId,
        TokenSetComposed? tokens,
        SchemaId schemaId,
        JsonDocument value,
        CancellationToken cancellationToken = default)
    {
        InternalReleases.EnsureReleaseDoesntExist(releaseId);
        var schema = ParentSection.GetSchema(schemaId);
        var tokenValues = tokens?.ToValueDictionary() ?? new Dictionary<string, JToken>();

        // System.Text.Json is immutable, which we like.
        // - NJsonSchema requires newtonsoft.
        // - Resolve requires mutable objects.
        // so, using System.Text.Json as much as possible.
        // but here we need to convert to JsonNet, then back.
        var newtonValue = value.ToJsonNetJson();
        var newtonResolvedValue = await JsonUtility.ResolveAsync(newtonValue, tokenValues, cancellationToken);
        
        ValidateAgainstSchema(newtonResolvedValue, schema.Schema);
        var resolvedValue = newtonResolvedValue.ToSystemTextJson();
        var tokensInUse = JsonUtility.GetTokenNamesDeep(newtonValue, tokenValues).ToHashSet();
        var evt = new ReleaseCreatedSourceEvent(
            releaseId,
            Id,
            ParentSection.Id,
            schemaId,
            value,
            resolvedValue,
            tokens,
            tokensInUse);
        ParentSection.PlaySourceEvent(evt);
        return GetRelease(releaseId);
    }

    private static void ValidateAgainstSchema(JToken value, JsonSchema schema)
    {
        var results = schema.Validate(value);
        if (results.Any())
        {
            throw new SchemaValidationFailedException(results);
        }
    }

    public SectionEntity ParentSection { get; }

    public string EnvironmentName { get; }
}