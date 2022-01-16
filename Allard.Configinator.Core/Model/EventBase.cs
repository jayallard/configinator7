namespace Allard.Configinator.Core.Model;

public record DomainEventBase : IDomainEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}