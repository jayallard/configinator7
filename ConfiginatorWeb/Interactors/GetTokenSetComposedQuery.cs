using System.Text;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Json;
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
        
        // todo: doesn't belong here, but is convenient.
        // build the mermaid markup for the token sets
        var mermaid = new StringBuilder().AppendLine("graph BT");
        AddChildren(tokenSet.Root);
        return new TokenSetComposedQueryResult(TokenSetComposedDto.FromTokenSetComposed(tokenSet), mermaid.ToString());

        void AddChildren(TokenSetComposed parent)
        {
            foreach (var child in parent.Children)
            {
                mermaid.AppendLine($"{child.TokenSetName} --> {parent.TokenSetName}");
                AddChildren(child);
            }
        }
    }
}

public record TokenSetComposedQuery(string TokenSetName) : IRequest<TokenSetComposedQueryResult>;
public record TokenSetComposedQueryResult(TokenSetComposedDto TokenSet, string MermaidMarkup);