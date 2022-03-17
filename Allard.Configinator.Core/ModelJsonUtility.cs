using System.Text.Json;
using System.Text.Json.Serialization;
using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;
using NuGet.Versioning;

namespace Allard.Configinator.Core;

public static class ModelJsonUtility
{
    private static readonly JsonSerializerOptions ModelSerializerOptions = CreateJsonSerializerOptions();

    public static JsonSerializerOptions CreateJsonSerializerOptions() => new()
    {
        Converters =
        {
            new SemanticVersionSerializer(),
            new IdConverterFactory(),
            new SchemaNameConverter()
        },
        WriteIndented = true
    };
    
    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, ModelSerializerOptions);

    public static T Deserialize<T>(string serialized) =>
        JsonSerializer.Deserialize<T>(serialized, ModelSerializerOptions);
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