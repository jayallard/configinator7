namespace Allard.DomainDrivenDesign;

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;

    //public abstract long EntityId { get; }
}