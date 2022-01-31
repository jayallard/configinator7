using Allard.Configinator.Core;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Configinator.Infrastructure;

namespace ConfiginatorWeb.Queries;

public class TokenSetQueriesCoreRepository : ITokenSetQueries
{
    private readonly ITokenSetRepository _repository;

    public TokenSetQueriesCoreRepository(ITokenSetRepository repository)
    {
        _repository = Guards.NotDefault(repository, nameof(repository));
    }

    public async Task<List<TokenSetListItemDto>> GetTokenSetListAsync(CancellationToken cancellationToken = default) =>
        (await _repository.FindAsync(new All(), cancellationToken))
            .Select(t => new TokenSetListItemDto { TokenSetName = t.TokenSetName})
            .ToList();

    // public async Task<TokenSetComposedDto?> GetTokenSetAsync(string tokenSetName,
    //     CancellationToken cancellationToken = default)
    // {
    //     var tokenSets = (await _repository.FindAsync(new All(), cancellationToken)).ToList();
    //     
    //     var sets = (await GetTokenSetListAsync(cancellationToken)).ToList();
    //     new TokenSetComposer()
    //     var tokenSet = (await _repository.FindAsync(new TokenSetNameIs(tokenSetName), cancellationToken))
    //         .SingleOrDefault();
    //     new TokenSetComposer()
    //     return TokenSetComposedDto.FromTokenSetComposed( tokenSet)
    // }
}