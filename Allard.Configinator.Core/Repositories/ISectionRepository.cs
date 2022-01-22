using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface ISectionRepository
{
    Task<SectionEntity?> GetAsync(SectionId id, CancellationToken cancellationToken);

    Task<IEnumerable<SectionEntity>> Find(ISpecification<SectionEntity> specification);
}