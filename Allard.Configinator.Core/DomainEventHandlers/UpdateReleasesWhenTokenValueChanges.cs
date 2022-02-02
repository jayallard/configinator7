using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;

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
        var sections =
            (await _unitOfWork.Sections.FindAsync(new UsesToken(evt.TokenSetName, evt.Key), cancellationToken))
            .ToList();

        var composer = await _tokenSetDomainService.GetTokenSetComposerAsync(cancellationToken);
        var relatedTokenSets = composer.GetDescendantsAndSelf(evt.TokenSetName).Select(t => t.TokenSetName).ToHashSet();
        
        foreach (var section in sections)
        {
            // get all the releases that use the token
            var releases = section
                .InternalEnvironments.SelectMany(
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
                            && r.TokensInUse.Contains(evt.Key))
                        .Select(r => new
                        {
                            Environment = e,
                            Release = r,
                        }));
            
            // todo: re-resolve the json value using the new token value
            // if the value has changed, then set the release to out of date
            foreach (var release in releases)
            {
                section.SetOutOfDate(release.Environment.Id, release.Release.Id, true);
            }
        }
    }
}