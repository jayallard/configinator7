using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.Model;

public class VariableSetAggregate : AggregateBase<VariableSetId>
{
    private readonly List<VariableSetId> _children = new();
    private readonly Dictionary<string, JToken> _variables = new();

    internal VariableSetAggregate(
        VariableSetId id,
        VariableSetId? baseId,
        // TODO: temporary
        string? baseVariableSetName,
        string @namespace,
        string variableSetName,
        string environmentType)
    {
        Play(new VariableSetCreatedEvent(id, baseId, baseVariableSetName, @namespace, variableSetName,
            environmentType));
    }

    internal VariableSetAggregate(List<IDomainEvent> events)
    {
        Guards.HasValue(events, nameof(events));
        foreach (var evt in events) Play(evt);
        InternalSourceEvents.Clear();
    }

    public IReadOnlyCollection<VariableSetId> Children => _children.AsReadOnly();
    public string VariableSetName { get; private set; }
    public string Namespace { get; private set; }

    public string? BaseVariableSetName { get; private set; }
    public VariableSetId BaseVariableSetId { get; private set; }

    public string EnvironmentType { get; private set; } = string.Empty;

    internal void AddOverride(VariableSetId overrideId)
    {
        Play(new VariableSetOverrideCreatedEvent(overrideId, Id));
    }

    internal void Play(IDomainEvent evt)
    {
        InternalSourceEvents.Add(evt);
        switch (evt)
        {
            case VariableSetOverrideCreatedEvent overrideCreated:
            {
                _children.Add(overrideCreated.VariableSetId);
                break;
            }
            case VariableSetCreatedEvent setCreated:
            {
                VariableSetName = setCreated.VariableSetName;
                Id = setCreated.VariableSetId;
                EnvironmentType = setCreated.EnvironmentType;
                BaseVariableSetName = setCreated.BaseVariableSetName;
                Namespace = setCreated.Namespace;
                break;
            }
            case VariableValueSetEvent setter:
            {
                _variables[setter.VariableName] = setter.Value;
                break;
            }
            case VariableValueCreatedEvent variableCreated:
            {
                _variables[variableCreated.VariableName] = variableCreated.Value;
                break;
            }
            default:
                throw new InvalidOperationException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    public VariableSet ToVariableSet()
    {
        return new VariableSet
        {
            BaseVariableSetName = BaseVariableSetName,
            VariableSetName = VariableSetName,
            Variables = _variables.ToDictionary(kv => kv.Key, kv => kv.Value.DeepClone())
        };
    }

    // TODO: change to async
    public void SetValue(string key, JToken value)
    {
        Guards.HasValue(value, nameof(value));
        if (_variables.ContainsKey(key))
        {
            Play(new VariableValueSetEvent(VariableSetName, key, value));
            return;
        }

        Play(new VariableValueCreatedEvent(VariableSetName, key, value));
    }
}