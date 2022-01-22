using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record SectionId : IdBase
{
    public SectionId(long id) : base(id)
    {
    }

}