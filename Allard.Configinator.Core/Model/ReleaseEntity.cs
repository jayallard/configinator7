using System.Text.Json;
using Allard.Json;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    // todo: make properties immutable
    internal List<DeploymentEntity> InternalDeployments { get; } = new();
    public IEnumerable<DeploymentEntity> Deployments => InternalDeployments.AsReadOnly();
    public JsonDocument ModelValue { get; }
    public JsonDocument ResolvedValue { get; }
    public TokenSetComposed? TokenSet { get; }
    public SchemaEntity Schema { get; }
    public DateTime CreateDate { get; }
    
    public bool IsDeployed { get; private set; }

    internal void SetDeployed(bool isDeployed) => IsDeployed = isDeployed;

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