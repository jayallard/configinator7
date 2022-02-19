using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications.Schema;

public class SchemaSectionIdIs : ISpecification<SchemaAggregate>
{
    public SchemaSectionIdIs(long sectionId)
    {
        SectionId = sectionId;
    }

    public long SectionId { get; }

    public bool IsSatisfied(SchemaAggregate obj)
    {
        return obj.SectionId?.Id == SectionId;
    }
}