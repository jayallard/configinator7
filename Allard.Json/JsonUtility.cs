using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class JsonUtility
{
    /// <summary>
    /// Parts of a string that begin and end with $$.
    /// </summary>
    private static readonly Regex VariableRegex = new(@"\$\$(.*?)\$\$", RegexOptions.Compiled);

    /// <summary>
    ///     Returns all variables found the document.
    ///     Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IEnumerable<(string VariableName, string JsonPath)> GetVariables(JObject json) =>
        json
            .Descendants()
            .SelectMany(GetVariables);

    /// <summary>
    ///     Return the name and path of the variable within the value.
    ///     If the value isn't a variable, it returns null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static IEnumerable<(string VariableName, string JsonPath)> GetVariables(JToken value)
    {
        // if not a string, there aren't any variables.
        if (value.Type != JTokenType.String) return Array.Empty<ValueTuple<string, string>>();
        
        // find the matches
        var variables = VariableRegex.Matches(value.Value<string>()!);
        
        // if not matches, get out.
        if (!variables.Any()) return Array.Empty<ValueTuple<string, string>>();

        // extract the variable names, and the json path.
        // all of the variables will have the same json path; they are
        // all coming from the same node.
        var result = variables
            .Select(v =>
                new ValueTuple<string, string>(v.Value.Replace("$$", string.Empty), value.Path))
            .ToArray();
        return result;
    }

    /// <summary>
    ///     Returns all of the variables used by a Json object.
    ///     Variables are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static HashSet<string> GetVariableNames(JObject json) =>
        GetVariables(json)
            .Select(t => t.VariableName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

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
        var refersTo = new Dictionary<string, ISet<string>?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, jsonNode) in variableElements)
        {
            // if we previously encountered this variable, then
            // move along
            if (refersTo.ContainsKey(key))
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
                refersTo.Add(key, variables);
                continue;
            }

            // if it's not an object, then there aren't any child variables.
            // add it to the dictionary so that we know it's a valid name,
            // but it doesn't have any children.
            refersTo.Add(key, null);
        }

        return refersTo;
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
            // need this because resolving one variable could result in pulling in more variables
            // todo: detect circular reference
            for (var i = 0; i < maxIterations; i++)
            {
                var variablesPerPath =
                    GetVariables(resolved)
                        .GroupBy(v => v.JsonPath)
                        .ToArray();

                if (!variablesPerPath.Any()) return resolved;
                foreach (var variablePath in variablesPerPath)
                {
                    var targetProperty = (JProperty) resolved.SelectToken(variablePath.Key)!.Parent!;
                    if (variablePath.Count() == 1)
                    {
                        // if the value is exactly a single variable, then set the value
                        var processed = ProcessValueWithSingleVariable(
                            variablePath.First().VariableName,
                            variablePath.First().JsonPath, 
                            resolved);
                        if (processed) continue;
                    }

                    // it's a string with multiple variables
                    var value = SubstituteScalarValues(targetProperty, variablePath);
                    targetProperty.Value = value;
                }
            }

            throw new InvalidOperationException($"Unable to resolve within {maxIterations} iterations.");
        }

        // there is only one variable at the Json Path.
        // the value is either exactly the one variable, or is a string with the variable embedded.
        // IE:  "$$first$$" or "blah blah $$first$$ blah blah"
        // if it's EXACTLY the one variable, then this will process it and return true.
        // otherwise, it will return false so the caller can proceed with
        // substitution.
        bool ProcessValueWithSingleVariable(string variableName, string jsonPath, JToken resolved)
        {
            var property = resolved.SelectToken(jsonPath);
            var propertyValue = property.Value<string>().Trim();
            var variableValue = GetVariableValue(variableName);

            // Yes: "$$first$$" - trimmed
            // Not: "hi there $$v$$"
            // Not: "$$a$$ $$b$$"
            var valueIsExactlyOneToken = propertyValue
                .Equals("$$" + variableName + "$$", StringComparison.OrdinalIgnoreCase);
            if (!valueIsExactlyOneToken) return false;
            ((JProperty) property.Parent).Value = variableValue.DeepClone();
            return true;

        }

        // get the variable from the dictionary.
        // if it doesn't exist, throw an exception.
        JToken GetVariableValue(string variableName)
        {
            if (variables.TryGetValue(variableName, out var value)) return value;
            throw new InvalidOperationException("The variable doesn't exist: " + variableName);
        }

        // a string value with more than one variable.  IE:  "$$last$$, $$first$$ $$middle$$"        
        string SubstituteScalarValues(JProperty targetProperty,
            IEnumerable<(string VariableName, string JsonPath)> variablesForPath)
        {
            // get the original value
            var value = targetProperty.Value.Value<string>()!;

            // iterate all the tokens for this path,
            // and make the substitutions.
            foreach (var (variableName, _) in variablesForPath)
            {
                var variableValue = GetVariableValue(variableName);
                EnsureIsScalar(variableName, variableValue);
                value = value.Replace(
                    "$$" + variableName + "$$",
                    variableValue.Value<string>(),
                    StringComparison.OrdinalIgnoreCase);
            }

            return value;
        }
    }

    private static void EnsureIsScalar(string variableName, JToken value)
    {
        if (IsScalar(value.Type)) return;
        throw new InvalidOperationException(
            $"The value of $${variableName}$$ must be a scalar.");
    }

    private static bool IsScalar(JTokenType type) =>
        // TODO: guid, etc.
        type is JTokenType.String
            or JTokenType.Boolean
            or JTokenType.Integer
            or JTokenType.Float;
}