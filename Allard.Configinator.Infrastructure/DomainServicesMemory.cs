using Allard.Configinator.Core.Queries;
using Allard.Configinator.Core.Services;

namespace Allard.Configinator.Infrastructure;

public class DomainServicesMemory : IDomainServices
{
    private readonly ISectionQueries _queries;

    public DomainServicesMemory(ISectionQueries queries)
    {
        _queries = queries;
    }

    public async Task EnsureSectionDoesntExistAsync(string sectionName)
    {
        if (await _queries.SectionsExistsAsync(sectionName))
            throw new InvalidOperationException("Section already exists: " + sectionName);
    }
}