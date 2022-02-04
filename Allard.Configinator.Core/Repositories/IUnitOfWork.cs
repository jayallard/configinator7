using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    IDataChangeTracker<SectionAggregate, SectionId> Sections { get; }
    IDataChangeTracker<TokenSetAggregate, TokenSetId> TokenSets { get; }
    IDataChangeTracker<GlobalSchemaAggregate, GlobalSchemaId> GlobalSchemas { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}