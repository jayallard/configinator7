using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Core.Repositories;

public interface ISectionRepository
{
    Task<SectionEntity?> GetSectionAsync(SectionId id);

    Task<SectionEntity?> GetSectionAsync(string sectionName);

    Task AddSectionAsync(SectionEntity section);
}