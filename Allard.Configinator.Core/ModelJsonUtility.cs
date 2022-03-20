using System.Text.Json;
using System.Text.Json.Serialization;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

public static class ModelJsonUtility
{
    private static readonly JsonSerializerOptions ModelSerializerOptions = CreateJsonSerializerOptions();

    public static JsonSerializerOptions CreateJsonSerializerOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new SemanticVersionSerializer(),
            new IdConverterFactory(),
            new SchemaNameConverter(),
            new JsonStringEnumConverter(),
            new JsonTokenConverter()
        },
        WriteIndented = true
    };

    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, obj.GetType(), ModelSerializerOptions);

    public static T Deserialize<T>(string serialized) =>
        JsonSerializer.Deserialize<T>(serialized, ModelSerializerOptions);

    public static T Deserialize<T>(Type type, string serialized) =>
        (T) JsonSerializer.Deserialize(serialized, type, ModelSerializerOptions);
}

public class SemanticVersionSerializer : JsonConverter<SemanticVersion>
{
    public override SemanticVersion?
        Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        SemanticVersion.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToFullString());
}

public class EntityIdConverter<T> : JsonConverter<T> where T : IIdentity
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetInt64();
        return (T) Activator.CreateInstance(typeToConvert, value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}

public class IdConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IIdentity));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(SchemaId)) return new EntityIdConverter<SchemaId>();
        if (typeToConvert == typeof(NamespaceId)) return new EntityIdConverter<NamespaceId>();
        if (typeToConvert == typeof(VariableSetId)) return new EntityIdConverter<VariableSetId>();
        if (typeToConvert == typeof(SectionId)) return new EntityIdConverter<SectionId>();
        if (typeToConvert == typeof(EnvironmentId)) return new EntityIdConverter<EnvironmentId>();
        if (typeToConvert == typeof(ReleaseId)) return new EntityIdConverter<ReleaseId>();
        if (typeToConvert == typeof(DeploymentId)) return new EntityIdConverter<DeploymentId>();
        throw new InvalidOperationException("Unhandled type: " + typeToConvert.FullName);
    }
}

public class SchemaNameConverter : JsonConverter<SchemaName>
{
    public override SchemaName? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value == null ? null : SchemaName.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, SchemaName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.FullName);
    }
}

/// <summary>
/// Some values are JTokens (ie: variable set values).
/// The JToken could be any type: object, array, string, number, whatever.
/// JToken is a base class and can't be deserialized to.
/// The WRITE wraps the JToken with a JObject, and saves the JObject as
/// an escaped string.
/// Thew READ loads the string into a JObject, and returns the value,
/// which is the JToken.
/// </summary>
public class JsonTokenConverter : JsonConverter<JToken>
{
    public override JToken? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return JObject.Parse(value).Root["value"];
    }

    public override void Write(Utf8JsonWriter writer, JToken value, JsonSerializerOptions options)
    {
        var obj = new JObject {new JProperty("value", value)};
        writer.WriteStringValue(obj.ToString());
    }
}