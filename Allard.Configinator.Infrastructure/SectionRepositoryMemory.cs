using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure;

public class SectionRepositoryMemory : ISectionRepository
{
    private readonly DatabaseMemory _database;

    public SectionRepositoryMemory(DatabaseMemory database)
    {
        _database = database;
    }

    public Task<SectionEntity?> GetSectionAsync(SectionId id, CancellationToken cancellationToken)
    {
        var section = (SectionEntity?)_database.Sections[id];
        return Task.FromResult(section);
    }

    public Task<SectionEntity?> GetSectionAsync(string sectionName)
    {
        var section =
            _database.Sections.Values.SingleOrDefault(s =>
                s.SectionName.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(section);
    }

    public Task AddSectionAsync(SectionEntity section)
    {
        _database.Sections.Add(section.Id, section);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<SectionEntity>> GetSectionsAsync(CancellationToken cancellationToken)
    {
        var sections = _database.Sections.Values.AsEnumerable();
        return Task.FromResult(sections);
    }
}