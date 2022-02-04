using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record GlobalSchemaId : IdBase
{
    public GlobalSchemaId(long id) : base(id)
    {
    }
}