using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications.Schema;

public class SchemaIsGlobal : ISpecification<SchemaAggregate>
{
    public bool IsSatisfied(SchemaAggregate obj)
    {
        return obj.IsGlobalSchema;
    }
}