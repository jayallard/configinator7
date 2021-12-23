namespace Configinator7.Core.Model;

public class HabitatSchemaValidationResults : List<HabitatSchemaValidationResult>
{
    public bool IsValid => this.All(i => i.IsValid);

    public void EnsureValid()
    {
        if (IsValid) return;
        throw new SchemaValidationFailedException(this);
    }
}