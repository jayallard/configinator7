namespace Allard.DomainDrivenDesign;

public record TransactionContext
{
    public Guid TransactionId { get; } = Guid.NewGuid();
    public DateTimeOffset Date { get; } = DateTimeOffset.Now;
}