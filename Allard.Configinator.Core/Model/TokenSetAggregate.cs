using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class TokenSetAggregate : AggregateBase<TokenSetId>
{
    private readonly Dictionary<string, JToken> _tokens = new();
    
    internal TokenSetAggregate(TokenSetId id, string name, string? baseTokenSet = null)
    {
        Play(new TokenSetCreatedEvent(id, name, null, baseTokenSet));
    }
    
    internal TokenSetAggregate(List<IDomainEvent> events)
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
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
                Id = created.TokenSetId;
                break;
            }
            case TokenValueSetEvent setter:
            {
                _tokens[setter.TokenName] = setter.Value;
                break;
            }
            case TokenValueCreatedEvent creator:
            {
                _tokens[creator.TokenName] = creator.Value;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }
    
    public TokenSet ToTokenSet() => new()
    {
        BaseTokenSetName = BaseTokenSetName,
        TokenSetName = TokenSetName,
        Tokens = _tokens.ToDictionary(kv => kv.Key, kv => kv.Value.DeepClone())
    };

    public void SetValue(string key, JToken value)
    {
        Guards.NotDefault(value, nameof(value));
        if (_tokens.ContainsKey(key))
        {
            Play(new TokenValueSetEvent(TokenSetName, key, value));
            return;
        }
        
        Play(new TokenValueCreatedEvent(TokenSetName, key, value));
    }
}