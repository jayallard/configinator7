namespace Allard.Configinator.Core.Queries;

public interface ISectionQueries
{
    Task<long?> GetSectionId(string sectionName);

    public async Task<bool> SectionsExistsAsync(string sectionName)
    {
        return await GetSectionId(sectionName) != null;
    }
}