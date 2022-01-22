namespace Allard.DomainDrivenDesign;

public record IdBase : IIdentity
{
    public long Id { get; }

    protected IdBase(long id) => Id = id;
}