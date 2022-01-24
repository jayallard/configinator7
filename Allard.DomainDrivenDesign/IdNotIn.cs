namespace Allard.DomainDrivenDesign;

public class IdNotIn<TIdentity> : ISpecification<TIdentity>
    where TIdentity : IIdentity
{
    private readonly ISet<long> _except;

    public IdNotIn(IEnumerable<TIdentity> ids)
    {
        _except = ids.Select(id => id.Id).ToHashSet();
    }

    public bool IsSatisfied(TIdentity obj) =>
        !_except.Contains(obj.Id);
}