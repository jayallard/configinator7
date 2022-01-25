using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    IDataChangeTracker<SectionEntity, SectionId> Sections { get; }
    IDataChangeTracker<TokenSetEntity, TokenSetId> TokenSets { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}