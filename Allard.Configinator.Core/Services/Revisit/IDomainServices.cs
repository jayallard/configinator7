namespace Allard.Configinator.Core.Services.Revisit;

public interface IDomainServices
{
    Task EnsureSectionDoesntExistAsync(string sectionName);
}