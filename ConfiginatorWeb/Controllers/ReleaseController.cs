using Configinator7.Core.Model;
using ConfiginatorWeb.Models.Release;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace ConfiginatorWeb.Controllers;

public class ReleaseController : Controller
{
    private readonly SuperAggregate _aggregate;

    public ReleaseController(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    // GET
    public IActionResult Edit(string configurationSectionName, string habitatName)
    {
        var v = new ViewEditRelease
        {
            HabitatName = habitatName,
            ConfigurationSectionName = configurationSectionName,
            Schemas = _aggregate.TemporaryExposure[configurationSectionName]
                .Schemas
                .OrderByDescending(s => s.Version)
                .Select(s => new ViewSchema(
                    "schema-" + s.Version.ToFullString().Replace(".", "-"),
                    s.Version.ToFullString(),
                    s.Schema.ToJson()))
                .ToList()
        };
        return View(v);
    }

    [HttpPost]
    public async Task<CreateResponse> Create(string configurationSectionName, string habitatName, string version, string value)
    {
        try
        {
            var json = JObject.Parse(value);
            await _aggregate.CreateReleaseAsync(configurationSectionName, habitatName, SemanticVersion.Parse(version), json);
            return new CreateResponse(true, new List<string>());
        }
        catch (SchemaValidationFailedException vex)
        {
            return new CreateResponse(false, vex.Errors.Select(v => v.ToString()).ToList());
        }
        catch (Exception ex)
        {
            return new CreateResponse(false, new List<string> {ex.Message});
        }
    }

    [HttpPost]
    public async Task<DeployResponse> Deploy(string configurationSectionName, string habitatName, long releaseId)
    {
        _aggregate.Deploy(configurationSectionName, habitatName, releaseId);
        return new DeployResponse();
    }
    
    public IActionResult History(string configurationSectionName, string habitatName)
    {
        var h2 = _aggregate.TemporaryExposure[configurationSectionName]
            .Habitats
            .SingleOrDefault(h => string.Equals(habitatName, h.HabitatId.Name, StringComparison.OrdinalIgnoreCase))
            .Releases
            .SelectMany(r => r.Deployments.Select(d => new HistoryItem(d.DeploymentDate, d.Action == DeploymentAction.Set, d.Reason, r.SchemaVersion, r.ReleaseId)))
            .OrderBy(h => h.Date)
            .ToList();
        var view = new HistoryView(configurationSectionName, habitatName, h2.ToList());
        return View(view);
    }

    public record HistoryView(string ConfigurationSectionName, string HabitatName, List<HistoryItem> History);
    public record HistoryItem(DateTime Date, bool IsDeploymentAction, string Reason, SemanticVersion SchemaVersion, long ReleaseId);
    public record DeployResponse;

    public record CreateResponse(bool Success, List<string> Errors);
}