using Allard.Configinator.Core.Repositories;

namespace Allard.Configinator.Infrastructure;

public class UnitOfWorkMemory : IUnitOfWork
{
    public ISectionRepository SectionRepository { get; }

    public UnitOfWorkMemory(ISectionRepository sectionRepository)
    {
        SectionRepository = sectionRepository;
    }

    public Task SaveAsync()
    {
        return Task.CompletedTask;
    }
}