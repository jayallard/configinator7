namespace Allard.DomainDrivenDesign;

public class AndSpecification<TLeft, TRight> : ISpecification<TRight>
    where TRight : TLeft
{
    private readonly ISpecification<TLeft> _left;
    private readonly ISpecification<TRight> _right;

    public AndSpecification(ISpecification<TLeft> left, ISpecification<TRight> right)
    {
        _left = left;
        _right = right;
    }

    public bool IsSatisfied(TRight obj)
    {
        return _left.IsSatisfied(obj) && _right.IsSatisfied(obj);
    }
}