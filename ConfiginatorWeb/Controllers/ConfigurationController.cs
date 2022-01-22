using Allard.Configinator.Core;
using Allard.Configinator.Core.Repositories;
using ConfiginatorWeb.Models.Configuration;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ConfigurationController : Controller
{
    private readonly ISectionQueries _sectionQueries;

    public ConfigurationController(
        ISectionQueries projections)
    {
        _sectionQueries = Guards.NotDefault(projections, nameof(projections));
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var sections = await _sectionQueries.GetSectionsListAsync();
        var view = new IndexView(sections.ToList());
        return View(view);
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Display(string name)
    {
        /*
        var section = _aggregate.TemporaryExposureSections[name];
        var view = new ViewConfiguration
        {
            SectionName = section.Name,
            Path = section.Path,
            Schemas = section.Schemas.Select(s => new ViewSchema
            {
                Text = s.Schema.ToJson(),
                Version = s.Version
            }).ToList(),
            Environments = section.Environments.Select(h => new ViewEnvironment
            {
                EnvironmentName = h.EnvironmentId.Name,
                Releases = h.Releases.Select(r => new ViewRelease
                {
                    ReleaseId = r.ReleaseId,
                    Version = r.Schema.Version,
                    CreateDate = r.CreateDate,
                    IsDeployed = r.IsDeployed,
                    IsOutOfDate = r.IsOutOfDate,
                    TokenSetName = r.TokenSet?.TokenSetName
                })
                    .OrderByDescending(r => r.ReleaseId)
                    .ToList()
            }).ToList()
        };*/
        return View(null);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateConfiguration config)
    {
        /*
        if (!ModelState.IsValid) return View();

        try
        {
            _aggregate.CreateSection(config.Name, null, config.Path, null);
        }
        catch (JsonReaderException ex)
        {
            ModelState.AddModelError("schema", ex.Message);
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("error", ex.Message);
            return View();
        }*/

        return RedirectToAction("Index");
    }
}

public record IndexView(List<SectionView> Sections);