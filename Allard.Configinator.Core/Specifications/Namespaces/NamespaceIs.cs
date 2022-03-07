using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications.Namespaces;

public class NamespaceIs : ISpecification<NamespaceAggregate>
{
    public NamespaceIs(string @namespace)
    {
        Namespace = NamespaceUtility.NormalizeNamespace(@namespace);
    }

    public string Namespace { get; }

    public bool IsSatisfied(NamespaceAggregate obj)
    {
        return obj.Namespace.Equals(Namespace, StringComparison.OrdinalIgnoreCase);
    }
}