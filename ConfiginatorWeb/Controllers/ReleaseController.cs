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
    public IActionResult Edit(string sectionName, string environmentName)
    {
        var v = new ViewEditRelease
        {
            EnvironmentName = environmentName,
            SectionName = sectionName,
            Schemas = _aggregate.TemporaryExposure[sectionName]
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
    public async Task<CreateResponse> Create(string sectionName, string environmentName, string version, string value)
    {
        try
        {
            var json = JObject.Parse(value);
            await _aggregate.CreateReleaseAsync(sectionName, environmentName, SemanticVersion.Parse(version), json);
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
    public async Task<DeployResponse> Deploy(string sectionName, string environmentName, long releaseId)
    {
        _aggregate.Deploy(sectionName, environmentName, releaseId);
        return new DeployResponse();
    }
    
    public IActionResult History(string sectionName, string environmentName)
    {
        var h2 = _aggregate.TemporaryExposure[sectionName]
            .Environments
            .SingleOrDefault(h => string.Equals(environmentName, h.EnvironmentId.Name, StringComparison.OrdinalIgnoreCase))
            .Releases
            .SelectMany(r => r.Deployments.Select(d => new HistoryItem(d.DeploymentDate, d.Action == DeploymentAction.Set, d.Reason, r.SchemaVersion, r.ReleaseId)))
            .OrderBy(h => h.Date)
            .ToList();
        var view = new HistoryView(sectionName, environmentName, h2.ToList());
        return View(view);
    }

    public record HistoryView(string SectionName, string EnvironmentName, List<HistoryItem> History);
    public record HistoryItem(DateTime Date, bool IsDeploymentAction, string Reason, SemanticVersion SchemaVersion, long ReleaseId);
    public record DeployResponse;

    public record CreateResponse(bool Success, List<string> Errors);
}