using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public class VariableSetComposed
{
    private readonly Dictionary<string, JToken> _tokens;
    private readonly Dictionary<string, VariableComposed> _resolved = new();
    private readonly List<VariableSetComposed> _children = new();

    public Dictionary<string, JToken> Variables =>
        _tokens.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.DeepClone(),
            StringComparer.OrdinalIgnoreCase);

    public ImmutableDictionary<string, VariableComposed> VariablesResolved
    {
        get
        {
            ResolveAll();
            return _resolved.ToImmutableDictionary(
                kv => kv.Key,
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    private void ResolveAll()
    {
        foreach (var key in this.Keys) Resolve(key);
    }
    
    public VariableSetComposed Root => BaseVariableSet?.Root ?? this;

    public ISet<string> GetRelatedVariableSetNames()
    {
        var values = new HashSet<string>();

        // self and parents
        var current = this;
        while (current != null)
        {
            values.Add(current.VariableSetName);
            current = current.BaseVariableSet;
        }

        // descendants
        AddChildren(this);
        return values;

        void AddChildren(VariableSetComposed variableSet)
        {
            foreach (var child in variableSet.Children)
            {
                values.Add(child.VariableSetName);
                AddChildren(child);
            }
        }
    }

    internal void AddChild(VariableSetComposed child)
    {
        if (child.BaseVariableSet != null) throw new InvalidOperationException("bug");
        child.BaseVariableSet = this;
        _children.Add(child);
    }

    public IEnumerable<VariableSetComposed> Children => _children.ToImmutableList();

    public VariableSetComposed GetChild(string variableSetName) => _children.Single(c =>
        c.VariableSetName.Equals(variableSetName, StringComparison.OrdinalIgnoreCase));

    public ISet<string> Keys =>
        BaseVariableSet == null
            ? _tokens.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : _tokens.Keys.Union(BaseVariableSet.Keys).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public string VariableSetName { get; }

    internal VariableComposed? Resolve(string key)
    {
        // already resolved
        if (_resolved.ContainsKey(key)) return _resolved[key];
        var variableComposed = new VariableComposed(key)
        {
            VariableSet = this
        };
        var fromParent = BaseVariableSet?.Resolve(key);
        var existsInParent = fromParent != null;
        var existsHere = _tokens.ContainsKey(key);
        if (!existsHere && !existsInParent) return null;
        variableComposed.Base = fromParent;

        // if it exists only here, then ti's ADDED (the default value)
        // if it exists here and in the parent, then it's an OVERRIDE
        // if it exists in the parent, but not here, then it's inherited
        if (existsInParent)
        {
            variableComposed.Origin = existsHere
                ? VariableOrigin.Override
                : VariableOrigin.Inherited;
        }

        if (existsHere)
        {
            variableComposed.Value = _tokens[key];
        }
        else if (existsInParent)
        {
            variableComposed.Value = fromParent.Value;
        }

        _resolved[key] = variableComposed;
        return variableComposed;
    }

    public VariableComposed GetToken(string key)
    {
        var value = Resolve(key);
        if (value == null) throw new KeyNotFoundException(key);

        // todo: clone
        return value;
    }

    public VariableSetComposed? BaseVariableSet { get; internal set; }

    public VariableSetComposed(
        Dictionary<string, JToken> variables,
        string variableSetName)
    {
        VariableSetName = variableSetName;
        _tokens = variables.ToDictionary(
            kv => kv.Key,
            kv => kv.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}