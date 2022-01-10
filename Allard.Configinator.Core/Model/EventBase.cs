namespace Allard.Configinator.Core.Model;

public record SourceEventBase : ISourceEvent
{
    public DateTime EventDate { get; set; } = DateTime.Now;
}