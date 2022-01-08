using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Repositories;

public interface ISectionRepository
{
    Task<SectionAggregate?> GetSectionAsync(long id);

    Task<SectionAggregate?> GetSectionAsync(string sectionName);

    Task AddSectionAsync(SectionAggregate section);
}