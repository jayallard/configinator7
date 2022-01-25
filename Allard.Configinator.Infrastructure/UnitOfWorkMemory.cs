using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class UnitOfWorkMemory : IUnitOfWork
{
    public UnitOfWorkMemory(
        ISectionRepository sectionRepository, 
        ITokenSetRepository tokenSetRepository)
    {
        Sections = new DataChangeTracker<SectionEntity, SectionId>(sectionRepository);
        TokenSets = new DataChangeTracker<TokenSetEntity, TokenSetId>(tokenSetRepository);
    }
    
    public DataChangeTracker<SectionEntity, SectionId> Sections { get; } 
    public DataChangeTracker<TokenSetEntity, TokenSetId> TokenSets { get; } 

    // // public async Task<bool> Exists(ISpecification<SectionEntity> specification)
    // //     => _sections.Any(specification.IsSatisfied) || await _sectionRepository.ExistsAsync(specification);
    // //
    // // public async Task<bool> Exists(ISpecification<TokenSetEntity> specification)
    //     => _tokenSets.Any(specification.IsSatisfied) || await _tokenSetRepository.ExistsAsync(specification);

    // private async Task PublishSourceEventsFrom<TEntity, TIdentity>(IEnumerable<IAggregate<TIdentity>> entities,
    //     CancellationToken token)
    //     where TIdentity : IIdentity
    // {
    //     //var type = typeof(TEntity);
    //     foreach (var e in entities)
    //     {
    //         //await _eventSourceRepository.AppendAsync(type, e.Id.Id, e.SourceEvents, token);
    //         await _eventPublisher.PublishAsync(e.SourceEvents, token);
    //         e.ClearEvents();
    //     }
    // }

    // public async Task<List<SectionEntity>> GetSectionsAsync(ISpecification<SectionEntity> specification)
    // {
    //     // get from the db only those not already in memory.
    //     var notAlreadyInMemory = new SectionIdNotIn(_sections.Select(s => s.Id));
    //     var fromDb = await _sectionRepository.FindAsync(notAlreadyInMemory.And(specification));
    //
    //     // add the new ones to memory.
    //     _sections.AddRange(fromDb);
    //
    //     // execute the query against memory, which now has everything we need
    //     return _sections.Where(specification.IsSatisfied).ToList();
    // }
    //
    // public Task AddSectionAsync(SectionEntity section)
    // {
    //     _sections.Add(section);
    //     return Task.CompletedTask;
    // }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Sections.SaveChangesAsync(cancellationToken);
        await TokenSets.SaveChangesAsync(cancellationToken);
    }
}