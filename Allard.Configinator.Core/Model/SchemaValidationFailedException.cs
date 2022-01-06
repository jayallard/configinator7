using NJsonSchema.Validation;

namespace Allard.Configinator.Core.Model;

public class SchemaValidationFailedException : Exception
{
    public SchemaValidationFailedException(IEnumerable<ValidationError> errors)
    {
        Errors = errors.ToList();
    }

    public List<ValidationError> Errors { get; }  
}