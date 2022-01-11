using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class ExtensionMethods
{
    public static JsonDocument ToSystemTextJson(this JObject json) => JsonDocument.Parse(json.ToString());
    public static JObject ToJsonNetJson(this JsonDocument json) => JObject.Parse(json.RootElement.ToString());
}