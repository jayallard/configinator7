using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    UnitOfWorkDataset<SectionEntity, SectionId> Sections { get; }
    UnitOfWorkDataset<TokenSetEntity, TokenSetId> TokenSets { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}