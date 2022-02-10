using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record VariableSetId : IdBase
{
    public VariableSetId(long id) : base(id)
    {
    }
}