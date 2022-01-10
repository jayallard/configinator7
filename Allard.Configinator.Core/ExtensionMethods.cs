using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core;

public static class ExtensionMethods
{
    public static IDictionary<string, JToken> ToValueDictionary(this TokenSetComposed tokens) =>
        tokens
            .Tokens
            .Values
            .ToDictionary(t => t.Name, t => t.Token, StringComparer.OrdinalIgnoreCase);
}