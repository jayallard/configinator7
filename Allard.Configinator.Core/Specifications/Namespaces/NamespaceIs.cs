using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications.Namespaces;

public class NamespaceIs : ISpecification<NamespaceAggregate>
{
    public NamespaceIs(string @namespace)
    {
        Namespace = @namespace;
    }

    public string Namespace { get; }
    public bool IsSatisfied(NamespaceAggregate obj) =>
        obj.Namespace.Equals(Namespace, StringComparison.OrdinalIgnoreCase);
}