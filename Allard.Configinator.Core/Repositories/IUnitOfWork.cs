namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    ISectionRepository SectionRepository { get; }

    Task SaveAsync();
}