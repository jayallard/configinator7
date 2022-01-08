namespace Allard.Configinator.Core.Model;

public record EventBase : IEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}