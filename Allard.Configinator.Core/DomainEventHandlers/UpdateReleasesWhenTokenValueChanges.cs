namespace Allard.Configinator.Core.DomainEventHandlers;

public class UpdateReleasesWhenTokenValueChanges : IEventHandler<TokenValueSetEvent>
{
    public Task ExecuteAsync(TokenValueSetEvent evt, CancellationToken token = default)
    {
        var key = evt.Key;
        // get sections where token set and token are in use
        // flag the releases that use the tokens as out of date
        return Task.CompletedTask;
    }
}