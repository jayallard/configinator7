using Configinator7.Core.Model;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Projections;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NJsonSchema;
using NuGet.Versioning;

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
        var secret = _aggregate.TemporarySecretExposure[name];
        var view = new ViewConfiguration
        {
            Name = secret.Id.Name,
            Path = secret.Path,
            Schema = secret.Schemas.Last().Schema.ToJson()
        };
        return View(view);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateConfiguration config)
    {
        if (!ModelState.IsValid) return View();

        try
        {
            var schema = await JsonSchema.FromJsonAsync(config.Schema);
            var configSchema = new ConfigurationSchema(new SemanticVersion(1, 0, 0), schema);
            _aggregate.CreateSecret(config.Name, configSchema, config.Path, null);
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