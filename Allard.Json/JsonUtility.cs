using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class JsonUtility
{
    /// <summary>
    /// Returns all tokens found the document.
    /// Tokens are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IEnumerable<(string TokenName, string Path)> GetTokens(JObject json) =>
        json
            .Descendants()
            .Select(GetTokenNameAndPath)
            .Where(v => v != null)
            .Select(v => v!.Value);
    
    /// <summary>
    /// Return the name and path of the token within the value.
    /// If the value isn't a token, it returns null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static (string TokenName, string Path)? GetTokenNameAndPath(JToken value)
    {
        if (value.Type != JTokenType.String) return null;
        var v = value.Value<string>();
        return
            IsToken(v)
                ? new ValueTuple<string, string>(v!.Substring(2, v.Length - 4), value.Path)
                : null;
    }

    /// <summary>
    /// Returns true if the string value is a token.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsToken(string? value) => value != null &&
                                                  value.StartsWith("$$", StringComparison.OrdinalIgnoreCase)
                                                  && value.EndsWith("$$", StringComparison.OrdinalIgnoreCase);
    /// <summary>
    /// Returns all of the tokens used by a Json object.
    /// Tokens are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static ISet<string> GetTokenNames(JObject json) => GetTokens(json)
        .Select(t => t.TokenName)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a list of all tokens required by the value.
    /// Tokens are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="value">The json value that uses references.</param>
    /// <param name="tokenElements">Token names and values.</param>
    /// <returns>A list of unique token names required by the value.</returns>
    public static ISet<string> GetTokenNamesDeep(JObject value, IDictionary<string, JToken> tokenElements)
    {
        // for each token in token elements, get the tokens that it requires.
        // (tokens can use tokens, which use other tokens, etc. it can go as deep as we want)
        var references = GetReferencedTokens(tokenElements);

        // get the tokens referenced by the value, and all the tokens referenced by those tokens.
        return AddSelfAndReferencedTokens(GetTokenNames(value), new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        // adds the tokenName to the hash set.
        // also, adds all of the tokens that it references.
        ISet<string> AddSelfAndReferencedTokens(IEnumerable<string>? tokenNames, ISet<string> names)
        {
            if (tokenNames == null) return names;
            foreach (var tokenName in tokenNames)
            {
                if (names.Contains(tokenName))
                {
                    // already did this one.
                    // tokens may be used may times within an object
                    continue;
                }

                names.Add(tokenName);
                AddSelfAndReferencedTokens(references[tokenName], names);
            }

            return names;
        }
    }

    /// <summary>
    /// The value of a token may refer to other tokens. This extracts the referenced tokens.
    /// Tokens are string values of the format $$token-name$$.
    /// </summary>
    /// <param name="tokenElements">Key = token name, value = token value.</param>
    /// <returns>Key = the name of the token, Value = the token the value references.</returns>
    private static IDictionary<string, ISet<string>?> GetReferencedTokens(IDictionary<string, JToken> tokenElements)
    {
        var references = new Dictionary<string, ISet<string>?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, jsonNode) in tokenElements)
        {
            // if we previously encountered this token, then
            // move along
            if (references.ContainsKey(key))
            {
                // already processed this one
                continue;
            }

            // if the json node is an object, then extract the embedded token names.
            // IE:
            // value = { "MyObject": "$$stuff$$" }
            // $$stuff$$ = {  "FirstName": "$$first$$", "LastName": "$$last$$" }
            // This will extract $$first$$ and $$last$$.
            // The result is Key=Stuff, Children=first, last
            if (jsonNode.Type == JTokenType.Object)
            {
                var tokens = GetTokenNames((JObject) jsonNode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                references.Add(key, tokens);
                continue;
            }

            // if it's not an object, then there aren't any child tokens.
            // add it to the dictionary so that we know it's a valid name,
            // but it doesn't have any children.
            references.Add(key, null);
        }

        return references;
    }

    public static Task<JObject> ResolveAsync(
        JObject model,
        IDictionary<string, JToken> tokens,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(Resolve, cancellationToken);

        JObject Resolve()
        {
            const int maxIterations = 10;

            // make a copy of the model - this is the result value
            // get the tokens from the resolved doc
            // replace with the token values - each token value may itself contain more tokens
            // repeat until no more tokens remain
            var resolved = (JObject) model.DeepClone();

            // max of 10 iterations
            // todo: detect circular reference
            for (var i = 0; i < maxIterations; i++)
            {
                var remainingTokens =
                    GetTokens(resolved)
                        .ToList();

                if (!remainingTokens.Any())
                {
                    return resolved;
                }

                foreach (var (tokenName, path) in remainingTokens)
                {
                    var tokenValue = tokens[tokenName];
                    var property = (JProperty) resolved.SelectToken(path)!.Parent!;
                    property.Value = tokenValue.DeepClone();
                }
            }

            throw new InvalidOperationException($"Unable to resolve within {maxIterations} iterations.");
        }
    }
}