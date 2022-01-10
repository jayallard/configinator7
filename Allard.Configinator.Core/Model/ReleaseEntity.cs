using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class ReleaseEntity : EntityBase<ReleaseId>
{
    public ReleaseEntity(
        EnvironmentEntity parent,
        ReleaseId id, 
        ConfigurationSchema schema,
        JObject modelValue,
        JObject resolvedValue,
        TokenSetComposed? tokenSet) : base(id)
    {
    }
}