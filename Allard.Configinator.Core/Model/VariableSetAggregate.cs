using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class VariableSetAggregate : AggregateBase<VariableSetId>
{
    private readonly Dictionary<string, JToken> _variables = new();
    
    internal VariableSetAggregate(VariableSetId id, string name, string? baseVariableSet = null)
    {
        Play(new VariableSetCreatedEvent(id, name, null, baseVariableSet));
    }
    
    internal VariableSetAggregate(List<IDomainEvent> events)
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
    }

    public string VariableSetName { get; private set; }

    public string? BaseVariableSetName { get; private set; }

    private void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case VariableSetCreatedEvent created:
            {
                VariableSetName = created.VariableSetName;
                BaseVariableSetName = created.BaseVariableSetName;
                Id = created.VariableSetId;
                break;
            }
            case VariableValueSetEvent setter:
            {
                _variables[setter.VariableName] = setter.Value;
                break;
            }
            case VariableValueCreatedEvent creator:
            {
                _variables[creator.VariableName] = creator.Value;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }
    
    public VariableSet ToVariableSet() => new()
    {
        BaseVariableSetName = BaseVariableSetName,
        VariableSetName = VariableSetName,
        Variables = _variables.ToDictionary(kv => kv.Key, kv => kv.Value.DeepClone())
    };

    public void SetValue(string key, JToken value)
    {
        Guards.NotDefault(value, nameof(value));
        if (_variables.ContainsKey(key))
        {
            Play(new VariableValueSetEvent(VariableSetName, key, value));
            return;
        }
        
        Play(new VariableValueCreatedEvent(VariableSetName, key, value));
    }
}