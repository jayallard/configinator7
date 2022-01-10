using Allard.Json;

namespace Allard.Configinator.Core.Services;

public interface ITokenService
{
    Task<TokenSet> GetTokenSetAsync(string name, CancellationToken cancellationToken = default);
    Task<TokenSetComposed> GetTokenSetComposedAsync(string name, CancellationToken cancellationToken = default);
}