using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure;

public class UnitOfWorkMemory : IUnitOfWork
{
    public ISectionRepository Sections { get; }

    public UnitOfWorkMemory(ISectionRepository sectionRepository)
    {
        Sections = sectionRepository;
    }

    public Task SaveAsync()
    {
        return Task.CompletedTask;
    }
}