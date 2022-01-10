namespace Allard.Configinator.Core.Model;

public record SectionId : IdBase<SectionEntity>
{
    public SectionId(long id) : base(id)
    {
    }

}