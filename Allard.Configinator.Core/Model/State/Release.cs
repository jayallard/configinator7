using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model.State;

public record Release(
    long ReleaseId,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetComposed? TokenSet,
    HashSet<string> UsedTokens,
    ConfigurationSchema Schema,
    DateTime CreateDate)
{
    // todo: make properties immutable
    
    public List<Deployment> Deployments { get; } = new();
    public bool IsDeployed { get; set; }
    public bool IsOutOfDate { get; set; }
}