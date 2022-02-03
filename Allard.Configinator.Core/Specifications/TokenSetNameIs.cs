using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class TokenSetNameIs : ISpecification<TokenSetAggregate>
{
    public string Name { get; }

    public TokenSetNameIs(string name) => Name = name;

    public bool IsSatisfied(TokenSetAggregate obj) =>
        obj.TokenSetName.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
}