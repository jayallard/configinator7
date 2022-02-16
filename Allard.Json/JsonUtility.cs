using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class JsonUtility
{
    /// <summary>
    /// Returns all variables found the document.
    /// Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IEnumerable<(string VariableName, string JsonPath)> GetVariables(JObject json) =>
        json
            .Descendants()
            .Select(GetTokenNameAndPath)
            .Where(v => v != null)
            .Select(v => v!.Value);

    /// <summary>
    /// Return the name and path of the variable within the value.
    /// If the value isn't a variable, it returns null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static (string VariableName, string JsonPath)? GetTokenNameAndPath(JToken value)
    {
        if (value.Type != JTokenType.String) return null;
        var v = value.Value<string>();
        return
            IsToken(v)
                ? new ValueTuple<string, string>(v!.Substring(2, v.Length - 4), value.Path)
                : null;
    }

    /// <summary>
    /// Returns true if the string variable is a variable.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsToken(string? value) => value != null &&
                                                  value.StartsWith("$$", StringComparison.OrdinalIgnoreCase)
                                                  && value.EndsWith("$$", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns all of the variables used by a Json object.
    /// Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static HashSet<string> GetVariableNames(JObject json) => GetVariables(json)
        .Select(t => t.VariableName)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a list of all variables required by the value.
    /// Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="value">The json value that uses references.</param>
    /// <param name="variableElements">Token names and values.</param>
    /// <returns>A list of unique variable names required by the value.</returns>
    public static ISet<string> GetVariableNamesDeep(JObject value, IDictionary<string, JToken> variableElements)
    {
        // for each variable in variable elements, get the variables that it requires.
        // (variables can use other variables, which use other variables, etc. it can go as deep as we want)
        var references = GetReferencedVariables(variableElements);

        // get the variables referenced by the value, and all the variables referenced by those variables.
        return AddSelfAndReferencedVariables(GetVariableNames(value),
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        // adds the variable name to the hash set.
        // also, adds all of the variables that it references.
        ISet<string> AddSelfAndReferencedVariables(IEnumerable<string>? variableNames, ISet<string> names)
        {
            if (variableNames == null) return names;
            foreach (var variableName in variableNames)
            {
                if (names.Contains(variableName))
                {
                    // already did this one.
                    // variables may be used may times within an object
                    continue;
                }

                names.Add(variableName);
                AddSelfAndReferencedVariables(references[variableName], names);
            }

            return names;
        }
    }

    /// <summary>
    /// The value of a variable may refer to other variables. This extracts the referenced variables.
    /// Variables are string values of the format $$variable-name$$.
    /// </summary>
    /// <param name="variableElements">Key = variable name, value = variable value.</param>
    /// <returns>Key = the name of the variable, Value = the variable the value references.</returns>
    private static IDictionary<string, ISet<string>?> GetReferencedVariables(
        IDictionary<string, JToken> variableElements)
    {
        var references = new Dictionary<string, ISet<string>?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, jsonNode) in variableElements)
        {
            // if we previously encountered this variable, then
            // move along
            if (references.ContainsKey(key))
            {
                // already processed this one
                continue;
            }

            // if the json node is an object, then extract the embedded variable names.
            // IE:
            // value = { "MyObject": "$$stuff$$" }
            // $$stuff$$ = {  "FirstName": "$$first$$", "LastName": "$$last$$" }
            // This will extract $$first$$ and $$last$$.
            // The result is Key=Stuff, Children=first, last
            if (jsonNode.Type == JTokenType.Object)
            {
                var variables = GetVariableNames((JObject) jsonNode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                references.Add(key, variables);
                continue;
            }

            // if it's not an object, then there aren't any child variables.
            // add it to the dictionary so that we know it's a valid name,
            // but it doesn't have any children.
            references.Add(key, null);
        }

        return references;
    }

    public static string ToIndented(this JsonElement json) =>
        JsonSerializer.Serialize(json, new JsonSerializerOptions {WriteIndented = true});

    public static Task<JObject> ResolveAsync(
        JObject model,
        IDictionary<string, JToken> variables,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(Resolve, cancellationToken);

        JObject Resolve()
        {
            const int maxIterations = 10;

            // make a copy of the model - this is the result value
            // get the variables from the resolved doc
            // replace with the variable values - each variable value may itself contain more variables
            // repeat until no variables remain
            var resolved = (JObject) model.DeepClone();

            // max of 10 iterations
            // todo: detect circular reference
            for (var i = 0; i < maxIterations; i++)
            {
                var remainingVariables =
                    GetVariables(resolved)
                        .ToList();

                if (!remainingVariables.Any())
                {
                    return resolved;
                }

                foreach (var (tokenName, path) in remainingVariables)
                {
                    var variableName = variables[tokenName];
                    var property = (JProperty) resolved.SelectToken(path)!.Parent!;
                    property.Value = variableName.DeepClone();
                }
            }

            throw new InvalidOperationException($"Unable to resolve within {maxIterations} iterations.");
        }
    }
}