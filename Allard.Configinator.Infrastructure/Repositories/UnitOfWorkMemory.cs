using Allard.Configinator.Core;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure.Repositories;

public class UnitOfWorkMemory : IUnitOfWork
{
    public UnitOfWorkMemory(
        ISectionRepository sectionRepository, 
        ITokenSetRepository tokenSetRepository)
    {
        Sections = Guards.NotDefault(new DataChangeTracker<SectionEntity, SectionId>(sectionRepository), nameof(sectionRepository));
        TokenSets = Guards.NotDefault(new DataChangeTracker<TokenSetEntity, TokenSetId>(tokenSetRepository),nameof(tokenSetRepository));
    }
    
    public IDataChangeTracker<SectionEntity, SectionId> Sections { get; } 
    public IDataChangeTracker<TokenSetEntity, TokenSetId> TokenSets { get; } 

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await Sections.SaveChangesAsync(cancellationToken);
        await TokenSets.SaveChangesAsync(cancellationToken);
    }
}