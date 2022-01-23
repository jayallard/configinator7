namespace Allard.DomainDrivenDesign;

public class AndSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfied(T obj) => _left.IsSatisfied(obj) && _right.IsSatisfied(obj);
}