using Allard.Configinator.Core.Queries;

namespace Allard.Configinator.Infrastructure;

public class SectionQueriesMemory : ISectionQueries
{
    /* not using actual queries, projections yet */

    private readonly DatabaseMemory _database;

    public SectionQueriesMemory(DatabaseMemory database)
    {
        _database = database;
    }

    public Task<long?> GetSectionId(string sectionName)
    {
        return Task.FromResult((long?)0);
        // var id = _database
        //     .Sections
        //     .Values
        //     .SingleOrDefault(s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase))?.SectionId;
        // return Task.FromResult(id);
    }
}