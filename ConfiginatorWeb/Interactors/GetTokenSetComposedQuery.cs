using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ConfiginatorWeb.Interactors;

public class GetTokenSetComposedQuery : IRequestHandler<TokenSetComposedQuery, TokenSetComposedQueryResult>
{
    private readonly TokenSetDomainService _tokenSetDomainService;

    public GetTokenSetComposedQuery(TokenSetDomainService tokenSetDomainService)
    {
        _tokenSetDomainService = Guards.NotDefault(tokenSetDomainService, nameof(tokenSetDomainService));
    }

    public async Task<TokenSetComposedQueryResult> Handle(TokenSetComposedQuery request,
        CancellationToken cancellationToken)
    {
        var tokenSet = await _tokenSetDomainService.GetTokenSetComposedAsync(request.TokenSetName, cancellationToken);
        var mermaid = MermaidUtility.FlowChartForTokenSet(tokenSet, request.TokenSetName);
        var dto = TokenSetComposedDto.FromTokenSetComposed(tokenSet);
        return new TokenSetComposedQueryResult(dto, mermaid);
    }
}


public record TokenSetComposedQuery(string TokenSetName) : IRequest<TokenSetComposedQueryResult>;

public record TokenSetComposedQueryResult(TokenSetComposedDto TokenSet, string MermaidMarkup);