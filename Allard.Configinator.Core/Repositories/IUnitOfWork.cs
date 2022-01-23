using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    Task<List<SectionEntity>> GetSectionsAsync(ISpecification<SectionEntity> specification);

    Task AddSectionAsync(SectionEntity section);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> Exists(ISpecification<SectionEntity> specification);
}