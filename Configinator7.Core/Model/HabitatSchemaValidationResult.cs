using NJsonSchema.Validation;

namespace Configinator7.Core.Model;

public class HabitatSchemaValidationResult
{
    public HabitatId HabitatId { get; set; }
    public List<ValidationError> ValidationErrors { get; set; }
    public bool IsValid => ValidationErrors == null || ValidationErrors.Count == 0;

    public void EnsureValid()
    {
        if (IsValid) return;
        throw new InvalidOperationException("Validation failed");
    }
}