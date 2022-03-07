using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class JsonUtility
{
    private static readonly Regex _variableRegex = new(@"\$\$(.*?)\$\$", RegexOptions.Compiled);

    /// <summary>
    ///     Returns all variables found the document.
    ///     Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IEnumerable<(string VariableName, string JsonPath)> GetVariables(JObject json)
    {
        return json
            .Descendants()
            .SelectMany(GetVariables);
    }

    /// <summary>
    ///     Return the name and path of the variable within the value.
    ///     If the value isn't a variable, it returns null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static IEnumerable<(string VariableName, string JsonPath)> GetVariables(JToken value)
    {
        if (value.Type != JTokenType.String) return Array.Empty<ValueTuple<string, string>>();
        var variables = _variableRegex.Matches(value.Value<string>()!);
        if (!variables.Any()) return Array.Empty<ValueTuple<string, string>>();

        var result = variables
            .Select(v =>
                new ValueTuple<string, string>(v.Value.Replace("$$", string.Empty), value.Path))
            .ToArray();
        return result;
    }

    /// <summary>
    ///     Returns true if the string variable is a variable.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsVariable(string? value)
    {
        return value != null &&
               value.StartsWith("$$", StringComparison.OrdinalIgnoreCase)
               && value.EndsWith("$$", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Returns all of the variables used by a Json object.
    ///     Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static HashSet<string> GetVariableNames(JObject json)
    {
        return GetVariables(json)
            .Select(t => t.VariableName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Returns a list of all variables required by the value.
    ///     Variables are string values of the format $$token-name$$.
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
                    // already did this one.
                    // variables may be used may times within an object
                    continue;

                names.Add(variableName);
                AddSelfAndReferencedVariables(references[variableName], names);
            }

            return names;
        }
    }

    /// <summary>
    ///     The value of a variable may refer to other variables. This extracts the referenced variables.
    ///     Variables are string values of the format $$variable-name$$.
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
                // already processed this one
                continue;

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

    public static string ToIndented(this JsonElement json)
    {
        return JsonSerializer.Serialize(json, new JsonSerializerOptions {WriteIndented = true});
    }

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
                var x = GetVariables(resolved);
                var remainingVariables =
                    x
                        .GroupBy(v => v.JsonPath)
                        .ToArray();

                if (!remainingVariables.Any()) return resolved;
                foreach (var p in remainingVariables)
                {
                    // if there are multiple variables, then the node must be of type string.
                    // IE:   "MyValue": "$$aa$$ blah blah $$bb$$"
                    var node = (JProperty) resolved.SelectToken(p.Key)!.Parent!;
                    if (p.Count() > 1 && node.Value.Type != JTokenType.String)
                    {
                        throw new InvalidOperationException(
                            "Invalid substitution. Multiple variable are specified for the JSON path, but the node at the JSON path isn't a string. JSON Path=" +
                            p.Key);
                    }

                    // if there's one variable, and the value of the variable is an object, then
                    // replace the node.
                    // IE:     "Kafka": "$$kafka$$"      $$kafka$$ is an object.
                    // becomes:  "Kafka": { ... } 
                    if (p.Count() == 1)
                    {
                        var variable = variables[p.First().VariableName];
                        if (variable.Type == JTokenType.Object)
                        {
                            var property = resolved.SelectToken(p.First().JsonPath);
                            if (!property.Value<string>().Trim().Equals("$$" + p.First().VariableName + "$$",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException(
                                    "The variable is a node, but the property value is a string. Invalid value=" + property.Value<string>());
                            }

                            ((JProperty)property.Parent).Value = variable.DeepClone();
                            continue;
                        }
                    }


                    // get the original value
                    var value = node.Value.Value<string>();

                    // iterate all the tokens for this path,
                    // and make the substitutions.
                    foreach (var (variableName, _) in p)
                    {
                        var variableValue = variables[variableName];
                        if (variableValue.Type != JTokenType.String)
                            throw new InvalidOperationException(
                                "Invalid substitution. The variable value must be a string. TODO: elaborate");

                        value = value.Replace(
                            "$$" + variableName + "$$",
                            variableValue.Value<string>(),
                            StringComparison.OrdinalIgnoreCase);
                    }

                    node.Value = value;
                }
            }

            throw new InvalidOperationException($"Unable to resolve within {maxIterations} iterations.");
        }
    }
}