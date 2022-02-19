namespace Allard.DomainDrivenDesign;

public record IdBase : IIdentity
{
    protected IdBase(long id)
    {
        Id = id;
    }

    public long Id { get; }
}