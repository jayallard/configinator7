using Allard.Configinator.Core.Model;
using Allard.Configinator.Infrastructure;
using ConfiginatorWeb.Models;

namespace ConfiginatorWeb.Queries;

public class SectionQueriesDatabase : ISectionQueries
{
    private readonly IDatabase _db;

    public SectionQueriesDatabase(IDatabase db)
    {
        _db = db;
    }

    public Task<List<SectionListItemView>> GetSectionsListAsync(CancellationToken cancellationToken = default)
    {
        var results = _db
            .Sections
            .Values
            .Select(s => new SectionListItemView(s.Id.Id, s.SectionName, s.Path, s.TokenSetName))
            .ToList();
        return Task.FromResult(results);
    }

    public Task<SectionView> GetSectionAsync(long id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_db.Sections[new SectionId(id)].ToOutputDto());

    public Task<SectionView> GetSectionAsync(string name, CancellationToken cancellationToken = default)
    {
        var section = _db.Sections.Values.Single(s => s.SectionName.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(section.ToOutputDto());
    }
}