using Configinator7.Core.Model;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Models.Configuration;
using ConfiginatorWeb.Projections;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConfiginatorWeb.Controllers;

public class ConfigurationController : Controller
{
    private readonly SuperAggregate _aggregate;
    private readonly IConfigurationProjections _projections;

    public ConfigurationController(
        SuperAggregate aggregate,
        IConfigurationProjections projections)
    {
        _aggregate = aggregate;
        _projections = projections;
    }

    // GET
    public IActionResult Index()
    {
        var view = new IndexView
        {
            ConfigurationSections = _projections.GetSections(),
            TokenSets = _projections.GetTokenSets()
        };
        return View(view);
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Display(string name)
    {
        var section = _aggregate.TemporaryExposureSections[name];
        var view = new ViewConfiguration
        {
            SectionName = section.Id.Name,
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
                    TokenSetName = r.TokenSet?.TokenSetName
                })
                    .OrderByDescending(r => r.ReleaseId)
                    .ToList()
            }).ToList()
        };
        return View(view);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateConfiguration config)
    {
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
        }

        return RedirectToAction("Index");
    }
}