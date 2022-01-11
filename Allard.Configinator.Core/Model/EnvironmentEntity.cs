using Allard.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;

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

    public ReleaseEntity GetRelease(ReleaseId releaseId) => InternalReleases.Single(r => r.Id == releaseId);

    public async Task<ReleaseEntity> CreateReleaseAsync(ReleaseId releaseId,
        TokenSetComposed? tokens,
        SemanticVersion schemaVersion,
        JObject value,
        CancellationToken cancellationToken = default)
    {
        if (InternalReleases.Any(r => r.Id == releaseId))
            throw new InvalidOperationException("Release id already exists: " + releaseId.Id);
        
        var schema = ParentSection.GetSchema(schemaVersion);
        var tokenValues = tokens?.ToValueDictionary() ?? new Dictionary<string, JToken>();

        var resolvedValue = await JsonUtility.ResolveAsync(value, tokenValues, cancellationToken);
        ValidateAgainstSchema(resolvedValue, schema.Schema);
        var tokensInUse = JsonUtility.GetTokenNamesDeep(value, tokenValues).ToHashSet();
        var evt = new ReleaseCreatedSourceEvent(
            releaseId,
            ParentSection.SectionName,
            EnvironmentName,
            schema,
            (JObject) value.DeepClone(),
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