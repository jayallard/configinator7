using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;

namespace Allard.Configinator.Core.DomainServices;

public class TokenSetDomainService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;

    public TokenSetDomainService(IUnitOfWork unitOfWork, IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
    }

    public async Task<TokenSetEntity> CreateTokenSetAsync(string tokenSetName, string? baseTokenSetName = default, CancellationToken cancellationToken = default)
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
        var tokenSet = new TokenSetEntity(id, tokenSetName, baseTokenSetName);
        await _unitOfWork.TokenSets.AddAsync(tokenSet);
        return tokenSet;
    }
}