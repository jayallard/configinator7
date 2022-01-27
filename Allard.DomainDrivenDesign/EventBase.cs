namespace Allard.DomainDrivenDesign;

public record DomainEventBase : IDomainEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}