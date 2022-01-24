using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class TokenSetNameIs : ISpecification<TokenSetEntity>
{
    public string Name { get; }

    public TokenSetNameIs(string name) => Name = name;

    public bool IsSatisfied(TokenSetEntity obj) =>
        obj.TokenSetName.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
}