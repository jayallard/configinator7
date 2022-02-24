using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public record NamespaceId : IdBase
{
    public NamespaceId(long id) : base(id)
    {
    }
}