using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Json;

namespace Allard.Configinator.Core.DomainServices;

public class TokenSetDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public TokenSetDomainService(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = Guards.NotDefault(unitOfWork, nameof(unitOfWork));
        _identityService = Guards.NotDefault(identityService, nameof(identityService));
    }

    public async Task<TokenSetAggregate> CreateTokenSetAsync(string tokenSetName, string? baseTokenSetName = default, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.TokenSets.Exists(new TokenSetNameIs(tokenSetName)))
        {
            throw new InvalidOperationException("TokenSet already exists: " + tokenSetName);
        }

        if (baseTokenSetName is not null && !(await _unitOfWork.TokenSets.Exists(new TokenSetNameIs(baseTokenSetName))))
        {
            throw new InvalidOperationException("Base TokenSet doesn't exist: " + baseTokenSetName);
        }

        var id = await _identityService.GetId<TokenSetId>();
        var tokenSet = new TokenSetAggregate(id, tokenSetName, baseTokenSetName);
        await _unitOfWork.TokenSets.AddAsync(tokenSet);
        return tokenSet;
    }

    public async Task<TokenSetComposer> GetTokenSetComposerAsync(CancellationToken cancellationToken = default)
    {
        var tokenSets = await _unitOfWork.TokenSets.FindAsync(new All(), cancellationToken);
        var x = tokenSets
            .Select(t => t.ToTokenSet())
            .ToList();
        return new TokenSetComposer(x);
    }

    public async Task<TokenSetComposed> GetTokenSetComposedAsync(string tokenSetName, CancellationToken cancellationToken = default) =>
            (await GetTokenSetComposerAsync(cancellationToken)).Compose(tokenSetName);
}