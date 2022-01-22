using Allard.Configinator.Infrastructure;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesDatabase : ISectionQueries
{
    private readonly IDatabase _db;

    public SectionQueriesDatabase(IDatabase db)
    {
        _db = db;
    }

    public Task<List<SectionView>> GetSectionsListAsync(CancellationToken cancellationToken = default)
    {
        var results = _db
            .Sections
            .Values
            .Select(s => new SectionView(s.Id.Id, s.SectionName, s.Path, s.TokenSetName))
            .ToList();
        return Task.FromResult(results);
    }
}