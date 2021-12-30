using NJsonSchema.Validation;

namespace Configinator7.Core.Model;

public class SchemaValidationFailedException : Exception
{
    public SchemaValidationFailedException(IEnumerable<ValidationError> errors)
    {
        Errors = errors.ToList();
    }

    public List<ValidationError> Errors { get; }  
}