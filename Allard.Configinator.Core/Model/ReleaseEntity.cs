using System.Text.Json;
using Allard.Json;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    // todo: make properties immutable
    internal List<DeploymentHistoryEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentHistoryEntity> Deployments => InternalDeployments.AsReadOnly();
    public JsonDocument ModelValue { get; }
    public JsonDocument ResolvedValue { get; }
    public TokenSetComposed? TokenSet { get; }
    public SchemaEntity Schema { get; }
    
    public DateTime CreateDate { get; }

    public ReleaseEntity(
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
    }
}