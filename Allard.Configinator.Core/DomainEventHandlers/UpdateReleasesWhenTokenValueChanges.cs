using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
/// When a token value is updated, then find all releases that use the token.
/// Resolve the release's model value against the current versions of the token sets.
/// If the resolved value is different than the release's resolved value, then the
/// release is out of date.
/// For example: When the release is created, the value of the PASSWORD token is ABC.
/// The password token value is changed to XYZ.
/// The existing release is now out of date because the password has changed.
/// </summary>
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
        Console.WriteLine("Token changed: " + evt);

        // get the composer for all token sets
        var tokenSet = await _tokenSetDomainService.GetTokenSetComposedAsync(evt.TokenSetName, cancellationToken);
        var values = tokenSet.ToValueDictionary();
        var related = tokenSet.GetRelatedTokenSetNames();

        // get all releases that use the token set and token
        var releases =
            await GetReleasesThatUseToken(evt.TokenSetName, evt.TokenName, related, cancellationToken);
        foreach (var release in releases)
        {
            // use the release's ModelValue, and the current TokenSet values.
            // Resolve, and see if the new value is different than the release's value.
            // if so, then the release is out of date.
            var resolved = await JsonUtility.ResolveAsync(release.Release.ModelValue.ToJsonNetJson(), values,
                cancellationToken);
            var changed = !JToken.DeepEquals(resolved, release.Release.ResolvedValue.ToJsonNetJson());
            release.Section.SetOutOfDate(release.Environment.Id, release.Release.Id, changed);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the names of all token sets that are
    /// descendants of tokenSetName, and also
    /// the tokenSetName.
    /// </summary>
    /// <param name="tokenSetName"></param>
    /// <param name="composer"></param>
    /// <returns></returns>
    // private static HashSet<string> GetRelatedTokenSetNames(string tokenSetName, TokenSetComposer composer) =>
    //     composer
    //         .GetDescendantsAndSelf(tokenSetName)
    //         .Select(t => t.TokenSetName)
    //         .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private async Task<IEnumerable<SectionAggregate>> GetAffectedSections(
        string tokenSetName,
        string tokenName,
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
        ISet<string> relatedTokenSets,
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
                            r.TokenSetId != null

                            // the token set is in the hierarchy of the token set that changed.
                            // IE: if there's a hierarchy of token sets, then the change
                            // can affect any release that's using any descendant of the changed
                            // token set.
                            // TODO fix
                            //&& relatedTokenSets.Contains(r.TokenSet.TokenSetName)

                            // the release is using the token that changed
                            && r.TokensInUse.Contains(tokenName))
                        .Select(r => new ReleaseDetails(s, e, r))
                ));
    }

    private record ReleaseDetails(SectionAggregate Section, EnvironmentEntity Environment, ReleaseEntity Release);
}