namespace Configinator7.Core.Model;

public class SchemaValidationFailedException : Exception
{
    public SchemaValidationFailedException(HabitatSchemaValidationResults results)
    {
        Results = results;
    }

    public HabitatSchemaValidationResults Results { get; }  
}