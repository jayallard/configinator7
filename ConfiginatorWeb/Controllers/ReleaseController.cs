using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ReleaseController : Controller
{
    private readonly ISectionQueries _sectionQueries;
    private readonly IMediator _mediator;

    public ReleaseController(
        ISectionQueries sectionQueries,
        IMediator mediator)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
    }

    //

    // // GET

    // public IActionResult Add(string sectionName, string environmentName)

    // {

    //     var section = _aggregate.TemporaryExposureSections[sectionName];

    //     var env = section.Environments.Single(e =>

    //         string.Equals(e.EnvironmentId.Name, environmentName, StringComparison.OrdinalIgnoreCase));

    //     

    //     // set the value to the last of the most recent release.

    //     var value = env.Releases.LastOrDefault()?.ModelValue.ToString();

    //     var ts = env.Releases.LastOrDefault()?.TokenSet?.TokenSetName;

    //     var v = new EditReleaseView

    //     {

    //         EnvironmentName = environmentName,

    //         SectionName = sectionName,

    //         Schemas = section

    //             .Schemas

    //             .OrderByDescending(s => s.Version)

    //             .Select(s => new EditSchemaView(

    //                 "schema-" + s.Version.ToFullString().Replace(".", "-"),

    //                 s.Version.ToFullString(),

    //                 s.Schema.ToJson()))

    //             .ToList(),

    //         DefaultValue = value,

    //         DefaultTokenSetName = ts,

    //         TokenSetNames = _aggregate.TemporaryExposureTokenSets.Keys.OrderBy(k => k).ToList()

    //     };

    //     return View(v);

    // }

    //

    // [HttpPost]

    // public async Task<CreateResponse> Create(string sectionName, string environmentName, string version, string value, string? tokenSetName)

    // {

    //     try

    //     {

    //         var json = JObject.Parse(value);

    //         await _aggregate.CreateReleaseAsync(sectionName, environmentName, tokenSetName, SemanticVersion.Parse(version), json);

    //         return new CreateResponse(true, new List<string>());

    //     }

    //     catch (SchemaValidationFailedException vex)

    //     {

    //         return new CreateResponse(false, vex.Errors.Select(v => v.ToString()).ToList());

    //     }

    //     catch (Exception ex)

    //     {

    //         return new CreateResponse(false, new List<string> {ex.Message});

    //     }

    // }

    //

    [HttpPost]
    public async Task<ReleaseDeployResponse> Deploy(ReleaseDeployRequest request, CancellationToken cancellationToken) =>
        await _mediator.Send(request, cancellationToken);


    public async Task<IActionResult> History(string sectionName, string environmentName,
        CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionName, cancellationToken);
        var env = section.GetEnvironment(environmentName);
        var history = env
            .Releases
            .SelectMany(r =>
                r.Deployments.Select(h => new ReleaseHistoryDeploymentPair(r, h)))
            .OrderBy(r => r.Deployment.DeploymentDate)
            .ToList();
        var view = new ReleaseHistoryView(section, env, history);
        return View(view);
    }

    public async Task<IActionResult> Display(string sectionName, string environmentName, long releaseId)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionName);
        var env = section.GetEnvironment(environmentName);
        var release = env.GetRelease(releaseId);
        var schema = section.GetSchema(release.Schema.Version);
        return View(new ReleaseDisplayView(section, env, release, schema));
    }

    //

    // public record HistoryView(string SectionName, string EnvironmentName, List<HistoryItem> History);

    //

    // public record HistoryItem(

    //     DateTime Date, 

    //     bool IsDeploymentAction, 

    //     string Reason, 

    //     SemanticVersion SchemaVersion,

    //     long ReleaseId,

    //     bool IsOutOrDate)

    // {

    //     public bool IsDeployed { get; set; }   

    // }

    // public record DeployResponse;

    //

    // public record CreateResponse(bool Success, List<string> Errors);
}

public record ReleaseDisplayView(
    SectionDto Section,
    SectionEnvironmentDto SelectedEnvironment,
    SectionReleaseDto SelectedRelease,
    SectionSchemaDto SelectedSchema);

public record ReleaseHistoryView(
    SectionDto SelectedSection,
    SectionEnvironmentDto SelectedEnvironment,
    List<ReleaseHistoryDeploymentPair> Deployments);

public record ReleaseHistoryDeploymentPair(
    SectionReleaseDto Release,
    SectionDeploymentDto Deployment);

public class ReleaseDeployRequest : IRequest<ReleaseDeployResponse>
{
    public string SectionName { get; set; }
    public string EnvironmentName { get; set; }
    public long ReleaseId { get; set; }
}

public class ReleaseDeployResponse
{
}