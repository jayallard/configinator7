namespace Allard.DomainDrivenDesign;

public interface IDomainEvent
{
    DateTime EventDate { get; }
}