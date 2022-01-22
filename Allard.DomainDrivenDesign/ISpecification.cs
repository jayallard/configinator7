namespace Allard.DomainDrivenDesign;

public interface ISpecification<in T>
{
    bool IsSatisfied(T obj);
}