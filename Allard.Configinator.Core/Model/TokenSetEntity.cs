using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class TokenSetEntity : AggregateBase<TokenSetId>
{
    private readonly Dictionary<string, JToken> _tokens = new();

    internal TokenSetEntity(TokenSetId id, string name, string? baseTokenSet = null) : base(id)
    {
        Play(new TokenSetCreatedEvent(id, name, null, baseTokenSet));
    }

    public string TokenSetName { get; private set; }

    public string? BaseTokenSetName { get; private set; }

    private void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case TokenSetCreatedEvent created:
            {
                TokenSetName = created.TokenSetName;
                BaseTokenSetName = created.BaseTokenSetName;
                break;
            }
            case TokenValueSetEvent setter:
            {
                _tokens[setter.TokenName] = setter.Value;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }
    
    public TokenSet ToTokenSet() => new()
    {
        Base = BaseTokenSetName,
        TokenSetName = TokenSetName,
        Tokens = _tokens.ToDictionary(kv => kv.Key, kv => kv.Value.DeepClone())
    };

    public void SetValue(string key, JToken value)
    {
        Guards.NotDefault(value, nameof(value));
        Play(new TokenValueSetEvent(TokenSetName, key, value));
    }
}