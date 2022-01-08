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

    public Task<SectionAggregate> GetSectionAsync(long id)
    {
        if (_database.Sections.TryGetValue(id, out var section))
        {
            return Task.FromResult(section);
        }

        throw new InvalidOperationException("doesn't exist");
    }

    public Task AddSectionAsync(SectionAggregate section)
    {
        _database.Sections.Add(section.SectionId, section);
        return Task.CompletedTask;
    }
}