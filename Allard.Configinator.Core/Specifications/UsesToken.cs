using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

/// <summary>
/// Find all sections which have a release that
/// uses the given token.
/// </summary>
public class UsesToken : ISpecification<SectionEntity>
{
    public UsesToken(string tokenSetName,string tokenName)
    {
        TokenSetName = tokenSetName;
        TokenName = tokenName;
    }

    public string TokenSetName { get; }
    public string TokenName { get; }

    public bool IsSatisfied(SectionEntity obj) =>
        // any environment that has any release that's using the token
        obj.Environments.Any(
            e => e.Releases.Any(r => string.Equals(TokenSetName, r.TokenSet?.TokenSetName, StringComparison.OrdinalIgnoreCase) && r.TokensInUse.Contains(TokenName)));
}