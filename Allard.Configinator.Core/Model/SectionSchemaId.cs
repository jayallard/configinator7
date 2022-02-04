using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record SectionSchemaId : IdBase
{
    public SectionSchemaId(long id) : base(id)
    {
    }
}