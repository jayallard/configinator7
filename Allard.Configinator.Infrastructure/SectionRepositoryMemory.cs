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

    public Task<SectionEntity> GetSectionAsync(SectionId id)
    {
        if (_database.Sections.TryGetValue(id, out var section))
        {
            return Task.FromResult(section);
        }

        throw new InvalidOperationException("doesn't exist");
    }

    public Task<SectionEntity?> GetSectionAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<SectionEntity?> GetSectionAsync(string sectionName)
    {
        throw new NotImplementedException();
    }

    public Task AddSectionAsync(SectionEntity section)
    {
        _database.Sections.Add(section.Id, section);
        return Task.CompletedTask;
    }
}