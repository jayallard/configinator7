namespace Allard.Configinator.Core.Services;

public interface IDomainServices
{
    Task EnsureSectionDoesntExistAsync(string sectionName);
}