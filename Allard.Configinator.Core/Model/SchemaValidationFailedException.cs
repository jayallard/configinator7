using NJsonSchema.Validation;

namespace Allard.Configinator.Core.Model;

public class SchemaValidationFailedException : Exception
{
    public SchemaValidationFailedException(List<ValidationError> errors) 
        : base("Schema validation failed:\n" + string.Join("\n", errors.Select(e => " - " + e)))
    {
        Errors = errors.ToList();
    }

    public List<ValidationError> Errors { get; }  
}