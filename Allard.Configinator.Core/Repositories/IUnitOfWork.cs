namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    ISectionRepository Sections { get; }

    Task SaveAsync();
}