namespace Allard.Configinator.Core.Model;

public record SchemaId : IdBase<SchemaEntity>
{
    public SchemaId(long id) : base(id)
    {
    }
}