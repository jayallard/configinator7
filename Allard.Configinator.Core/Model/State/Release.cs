using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public record Release(
    long ReleaseId,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSetResolved? TokenSet,
    HashSet<string> UsedTokens,
    ConfigurationSchema Schema,
    DateTime CreateDate)
{
    public List<Deployment> Deployments { get; } = new();
    public bool IsDeployed { get; set; }
    
    public bool IsOutOfDate { get; set; }
}