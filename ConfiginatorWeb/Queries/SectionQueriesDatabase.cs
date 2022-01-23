using Allard.Configinator.Core.Model;
using Allard.Configinator.Infrastructure;

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

    public Task<SectionView> GetSectionAsync(long id, CancellationToken cancellationToken = default)
    {
        var section = _db.Sections[new SectionId(id)];
        var view = new SectionView
        {
            SectionName = section.SectionName,
            Path = section.Path,
            Environments = section.Environments.Select(e => new SectionEnvironmentView
            {
                EnvironmentName = e.EnvironmentName,
                Releases = e.Releases.Select(r => new SectionReleaseView
                {
                    ReleaseId = r.Id.Id,
                    SchemaVersion = r.Schema.Version,
                }).ToList()
            }).ToList(),
            Schemas = section.Schemas.Select(s => new SectionSchemaView
            {
                Schema = s.Schema.ToJson(),
                Version = s.Version
            }).ToList()
        };

        return Task.FromResult(view);
    }
}