using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class UnitOfWorkMemory : IUnitOfWork, IDisposable
{
    private readonly IEventPublisher _publisher;
    public UnitOfWorkMemory(
        ISectionRepository sectionRepository, 
        ITokenSetRepository tokenSetRepository, 
        IEventPublisher publisher)
    {
        _publisher = Guards.NotDefault(publisher, nameof(publisher));
        Sections = Guards.NotDefault(new DataChangeTracker<SectionEntity, SectionId>(sectionRepository), nameof(sectionRepository));
        TokenSets = Guards.NotDefault(new DataChangeTracker<TokenSetEntity, TokenSetId>(tokenSetRepository),nameof(tokenSetRepository));
    }
    
    public IDataChangeTracker<SectionEntity, SectionId> Sections { get; } 
    public IDataChangeTracker<TokenSetEntity, TokenSetId> TokenSets { get; } 

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events =
            (await Sections.GetEvents(cancellationToken))
            .Union(await TokenSets.GetEvents(cancellationToken))
            .OrderBy(e => e.EventDate);

        // write the changes, then publish events.
        await Sections.SaveChangesAsync(cancellationToken);
        await TokenSets.SaveChangesAsync(cancellationToken);
        // if db, commit here.
        
        // this is after the commit. if this fails, then data changed and
        // downstream systems won't get word.
        // alternative: publish to the db outbox before the commit,
        // then copy from outbox to publisher.
        await _publisher.PublishAsync(events, cancellationToken);
    }

    public void Dispose()
    {
        (Sections as IDisposable)?.Dispose();
        (TokenSets as IDisposable)?.Dispose();
    }
}