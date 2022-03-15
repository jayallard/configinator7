using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Json;

public static class ExtensionMethods
{
    public static JsonDocument ToSystemTextJson(this JObject json)
    {
        return JsonDocument.Parse(json.ToString());
    }

    public static JObject ToJsonNetJson(this JsonDocument json)
    {
        return JObject.Parse(json.RootElement.ToString());
    }

    // public static JToken ToJsonNetJson(this JsonElement json)
    // {
    //     return JToken.Parse(json.ToString());
    // }

    public static string PrettyPrint(this JsonDocument doc)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true});
        doc.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}