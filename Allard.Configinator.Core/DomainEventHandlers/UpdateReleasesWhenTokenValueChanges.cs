using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainEventHandlers;

public class UpdateReleasesWhenTokenValueChanges : IEventHandler<TokenValueSetEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TokenSetDomainService _tokenSetDomainService;

    public UpdateReleasesWhenTokenValueChanges(IUnitOfWork unitOfWork, TokenSetDomainService tokenSetDomainService)
    {
        _tokenSetDomainService = Guards.NotDefault(tokenSetDomainService, nameof(tokenSetDomainService));
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(TokenValueSetEvent evt, CancellationToken cancellationToken = default)
    {
        // get all sections that have a release that use the token
        var composer = await _tokenSetDomainService.GetTokenSetComposerAsync(cancellationToken);
        var composedTokens = composer.Compose(evt.TokenSetName).ToValueDictionary();
        var relatedTokenSets = composer
            .GetDescendantsAndSelf(evt.TokenSetName)
            .Select(t => t.TokenSetName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        var releases = await GetReleasesThatUseToken(evt.TokenSetName, evt.TokenName, relatedTokenSets, cancellationToken);
        foreach (var release in releases)
        {
            // use the release's ModelValue, and the current TokenSet values.
            // Resolve, and see if the new value is different than the release's value.
            // if so, then the release is out of date.
            var resolved = await JsonUtility.ResolveAsync(release.Release.ModelValue.ToJsonNetJson(), composedTokens, cancellationToken);
            var changed = !JToken.DeepEquals(resolved, release.Release.ResolvedValue.ToJsonNetJson());
            release.Section.SetOutOfDate(release.Environment.Id, release.Release.Id, changed);
        }
    }

    private async Task<IEnumerable<SectionEntity>> GetAffectedSections(string tokenSetName, string tokenName,
        CancellationToken cancellationToken) =>
        (await _unitOfWork.Sections.FindAsync(new UsesToken(tokenSetName, tokenName), cancellationToken))
        .ToList();

    /// <summary>
    /// Returns all releases that use the token set and token.
    /// </summary>
    /// <param name="tokenName"></param>
    /// <param name="relatedTokenSets"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="tokenSetName"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ReleaseDetails>> GetReleasesThatUseToken(
        string tokenSetName, 
        string tokenName,
        IReadOnlySet<string> relatedTokenSets,
        CancellationToken cancellationToken)
    {
        return (await GetAffectedSections(tokenSetName, tokenName, cancellationToken))
            .SelectMany(s =>
                // sections have environments, environments have releases, releases use token sets
                // get all the releases that use the changed token of the changed token set
                s.InternalEnvironments.SelectMany(
                    e => e.InternalReleases
                        .Where(r =>
                            // has a token set
                            r.TokenSet != null

                            // the token set is in the hierarchy of the token set that changed.
                            // IE: if there's a hierarchy of token sets, then the change
                            // can affect any release that's using any descendant of the changed
                            // token set.
                            && relatedTokenSets.Contains(r.TokenSet.TokenSetName)

                            // the release is using the token that changed
                            && r.TokensInUse.Contains(tokenName))
                        .Select(r => new ReleaseDetails(s, e, r))
                ));
    }

    private record ReleaseDetails(SectionEntity Section, EnvironmentEntity Environment, ReleaseEntity Release);
}