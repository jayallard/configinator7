using System.Text.Json;
using NJsonSchema.Validation;

namespace Allard.Configinator.Core.Model;

public class SchemaValidationFailedException : Exception
{
    public SchemaValidationFailedException(JsonDocument invalidJson, List<ValidationError> errors)
        : base("Schema validation failed:\n" + string.Join("\n", errors.Select(e => " - " + e)))
    {
        InvalidJson = invalidJson;
        Errors = errors.ToList().AsReadOnly();
    }

    public JsonDocument InvalidJson { get; }
    public IReadOnlyCollection<ValidationError> Errors { get; }
}