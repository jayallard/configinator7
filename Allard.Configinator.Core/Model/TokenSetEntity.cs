using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class TokenSetEntity : AggregateBase<TokenSetId>
{
    
    public TokenSetEntity(TokenSetId id, string name, string? baseTokenSet = null) : base(id)
    {
        Play(new TokenSetCreatedSourceEvent(id, name, null, baseTokenSet));
    }

    private void Play(ISourceEvent evt)
    {
        switch (evt)
        {
            case TokenSetCreatedSourceEvent created:
            {
                _tokenSet.TokenSetName = created.TokenSetName;
                _tokenSet.Base = created.BaseTokenSetName;
                break;
            }
            case TokenValueSetSourceEvent setter:
            {
                _tokenSet.Tokens[setter.Key] = setter.Value;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    private readonly TokenSet _tokenSet = new();
    public string TokenSetName => _tokenSet.TokenSetName;

    public Dictionary<string, JToken> Tokens => _tokenSet.Tokens.ToDictionary(kv => kv.Key, kv => kv.Value.DeepClone(),
        StringComparer.OrdinalIgnoreCase);

    public string? Base => _tokenSet.Base;

    public void SetValue(string key, JToken value)
    {
        Guards.NotDefault(value, nameof(value));
        Play(new TokenValueSetSourceEvent(_tokenSet.TokenSetName, key, value));
    }
}