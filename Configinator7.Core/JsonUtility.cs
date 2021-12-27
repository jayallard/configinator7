using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace Configinator7.Core;

public static class JsonUtility
{
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


    private static IEnumerable<(string TokenName, string Path)> GetTokens(JObject json)
    {
        // todo: should be able to do this with SelectTokens
        List<(string TokenName, string Path)> tokens = new();
        Parse(json.Properties());
        return tokens.OrderBy(t => t.Path.Length);

        void Parse(IEnumerable<JProperty> properties)
        {
            // TODO: arrays
            foreach (var p in properties)
            {
                if (p.Value.Type == JTokenType.Object)
                {
                    // recurse!
                    Parse(p.Value.Value<JObject>()!.Properties());
                    continue;
                }

                // we are only interested in string values
                if (p.Value.Type != JTokenType.String) continue;

                // TODO: substrings
                var value = p.Value.Value<string>();
                if (value!.StartsWith("$$") && value.EndsWith("$$"))
                {
                    // remove the $$ from each end
                    tokens.Add(new ValueTuple<string, string>(value.Substring(2, value.Length - 4), p.Path));
                }
            }
        }
    }
}