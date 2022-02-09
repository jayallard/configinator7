using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.Configinator.Infrastructure;
using ConfiginatorWeb.Interactors;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ConfiginatorWeb.Queries;

public class TokenSetQueriesCoreRepository : ITokenSetQueries
{
    private readonly ITokenSetRepository _repository;
    private readonly TokenSetDomainService _tokenSetDomainService;

    public TokenSetQueriesCoreRepository(ITokenSetRepository repository, TokenSetDomainService tokenSetDomainService)
    {
        _tokenSetDomainService = Guards.NotDefault(tokenSetDomainService, nameof(tokenSetDomainService));
        _repository = Guards.NotDefault(repository, nameof(repository));
    }

    public async Task<List<TokenSetListItemDto>> GetTokenSetListAsync(CancellationToken cancellationToken = default)
    {
        var list = (await _repository.FindAsync(new All(), cancellationToken))
            .Select(async t =>
            {
                // create mermaid diagram for each root token set
                // it's a root if it doesn't have a base
                if (t.BaseTokenSetName != null)
                    return new TokenSetListItemDto(t.TokenSetName, t.BaseTokenSetName, null);
                var composed = await _tokenSetDomainService.GetTokenSetComposedAsync(t.TokenSetName, cancellationToken);
                var mermaid = MermaidUtility.FlowChartForTokenSet(composed, t.TokenSetName);
                return new TokenSetListItemDto(t.TokenSetName, t.BaseTokenSetName, mermaid);
            }).ToList();
        await Task.WhenAll(list);
        return list.Select(l => l.Result).ToList();
    }
}