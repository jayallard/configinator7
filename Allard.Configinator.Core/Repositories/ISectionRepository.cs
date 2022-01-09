using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Repositories;

public interface ISectionRepository
{
    Task<SectionEntity?> GetSectionAsync(SectionId id, CancellationToken cancellationToken);

    Task<SectionEntity?> GetSectionAsync(string sectionName);

    Task AddSectionAsync(SectionEntity section);

    Task<IEnumerable<SectionEntity>> GetSectionsAsync(CancellationToken cancellationToken);
}