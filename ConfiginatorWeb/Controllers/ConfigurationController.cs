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
        return View(_projections.GetConfigurationSections());
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Display(string name)
    {
        var configurationSection = _aggregate.TemporaryExposure[name];
        var view = new ViewConfiguration
        {
            Name = configurationSection.Id.Name,
            Path = configurationSection.Path,
            Schemas = configurationSection.Schemas.Select(s => new ViewSchema
            {
                Text = s.Schema.ToJson(),
                Version = s.Version
            }).ToList(),
            Habitats = configurationSection.Habitats.Select(h => new ViewHabitat
            {
                Name = h.HabitatId.Name,
                Releases = h.Releases.Select(r => new ViewRelease
                {
                    ReleaseId = r.ReleaseId,
                    Version = r.SchemaVersion,
                    CreateDate = r.CreateDate,
                    IsDeployed = r.IsDeployed
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
            _aggregate.CreateConfigurationSection(config.Name, null, config.Path, null);
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