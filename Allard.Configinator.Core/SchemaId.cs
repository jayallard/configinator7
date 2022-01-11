using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core;

public record SchemaId : IdBase<SchemaEntity>
{
    public SchemaId(long id) : base(id)
    {
    }
}