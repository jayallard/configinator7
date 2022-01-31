using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Queries;
using MediatR;

namespace ConfiginatorWeb.Interactors;

public class GetTokenSetComposedQuery : IRequestHandler<TokenSetComposedQuery, TokenSetComposedQueryResult>
{
    private readonly TokenSetDomainService _tokenSetDomainService;

    public GetTokenSetComposedQuery(TokenSetDomainService tokenSetDomainService)
    {
        _tokenSetDomainService = Guards.NotDefault(tokenSetDomainService, nameof(tokenSetDomainService));
    }

    public async Task<TokenSetComposedQueryResult> Handle(TokenSetComposedQuery request, CancellationToken cancellationToken)
    {
        var tokenSet = await _tokenSetDomainService.GetTokenSetComposedAsync(request.TokenSetName, cancellationToken);
        return new TokenSetComposedQueryResult(TokenSetComposedDto.FromTokenSetComposed(tokenSet));
    }
}

public record TokenSetComposedQuery(string TokenSetName) : IRequest<TokenSetComposedQueryResult>;
public record TokenSetComposedQueryResult(TokenSetComposedDto TokenSet);