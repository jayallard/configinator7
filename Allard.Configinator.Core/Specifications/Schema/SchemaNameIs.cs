using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications.Schema;

public class SchemaNameIs : ISpecification<SchemaAggregate>
{
    public SchemaNameIs(SchemaName schemaName)
    {
        SchemaName = schemaName;
    }

    public SchemaName SchemaName { get; }

    // obj == SchemaName didn't work. what's throwing it off??
    public bool IsSatisfied(SchemaAggregate obj)
    {
        return obj.SchemaName == SchemaName;
    }

    public static SchemaNameIs Is(string schemaName)
    {
        return new SchemaNameIs(new SchemaName(schemaName));
    }

    public static SchemaNameIs Is(SchemaName schemaName)
    {
        return new SchemaNameIs(schemaName);
    }
}