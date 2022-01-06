using Allard.Configinator.Core.Model;
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
    public IActionResult Add(string sectionName, string environmentName)
    {
        var section = _aggregate.TemporaryExposureSections[sectionName];
        var env = section.Environments.Single(e =>
            string.Equals(e.EnvironmentId.Name, environmentName, StringComparison.OrdinalIgnoreCase));
        
        // set the value to the last of the most recent release.
        var value = env.Releases.LastOrDefault()?.ModelValue.ToString();
        var ts = env.Releases.LastOrDefault()?.TokenSet?.TokenSetName;
        var v = new EditReleaseView
        {
            EnvironmentName = environmentName,
            SectionName = sectionName,
            Schemas = section
                .Schemas
                .OrderByDescending(s => s.Version)
                .Select(s => new EditSchemaView(
                    "schema-" + s.Version.ToFullString().Replace(".", "-"),
                    s.Version.ToFullString(),
                    s.Schema.ToJson()))
                .ToList(),
            DefaultValue = value,
            DefaultTokenSetName = ts,
            TokenSetNames = _aggregate.TemporaryExposureTokenSets.Keys.OrderBy(k => k).ToList()
        };
        return View(v);
    }

    [HttpPost]
    public async Task<CreateResponse> Create(string sectionName, string environmentName, string version, string value, string? tokenSetName)
    {
        try
        {
            var json = JObject.Parse(value);
            await _aggregate.CreateReleaseAsync(sectionName, environmentName, tokenSetName, SemanticVersion.Parse(version), json);
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
        var h2 = _aggregate.TemporaryExposureSections[sectionName]
            .Environments
            .SingleOrDefault(h => string.Equals(environmentName, h.EnvironmentId.Name, StringComparison.OrdinalIgnoreCase))
            .Releases
            .SelectMany(r => r.Deployments.Select(d => new HistoryItem(d.DeploymentDate, d.Action == DeploymentAction.Deployed, d.Reason, r.Schema.Version, r.ReleaseId, r.IsOutOfDate)
            {
                IsDeployed = d.IsDeployed
            }))
            .OrderBy(h => h.Date)
            .ToList();
        var view = new HistoryView(sectionName, environmentName, h2.ToList());
        return View(view);
    }

    public IActionResult Display(string sectionName, string environmentName, long releaseId)
    {
        // TODO: convert to dto, projection, etc.
        // just using the actual RELEASE for now
        var release = _aggregate
            .TemporaryExposureSections[sectionName]
            .Environments.Single(e =>
                string.Equals(e.EnvironmentId.Name, environmentName, StringComparison.OrdinalIgnoreCase))
            .Releases.Single(r => r.ReleaseId == releaseId);
        
        var v = new DisplayView
        {
            EnviornmentName = environmentName,
            SectionName = sectionName,
            ReleaseId = releaseId,
            Release = release
        };
        
        return View(v);
    }

    public record HistoryView(string SectionName, string EnvironmentName, List<HistoryItem> History);

    public record HistoryItem(
        DateTime Date, 
        bool IsDeploymentAction, 
        string Reason, 
        SemanticVersion SchemaVersion,
        long ReleaseId,
        bool IsOutOrDate)
    {
        public bool IsDeployed { get; set; }   
    }
    public record DeployResponse;

    public record CreateResponse(bool Success, List<string> Errors);
}