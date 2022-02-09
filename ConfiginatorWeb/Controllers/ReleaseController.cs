using System.ComponentModel.DataAnnotations;
using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Models.Release;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ReleaseController : Controller
{
    private readonly ITokenSetQueries _tokenSetQueries;
    private readonly ISectionQueries _sectionQueries;
    private readonly IMediator _mediator;

    public ReleaseController(
        ISectionQueries sectionQueries,
        ITokenSetQueries tokenSetQueries,
        IMediator mediator)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _tokenSetQueries = tokenSetQueries;
    }

    // DEPLOY
    [HttpGet]
    public async Task<IActionResult> Deploy(long sectionId, long environmentId, long releaseId)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId);
        var environment = section.GetEnvironment(environmentId);
        var release = environment.GetRelease(releaseId);
        var view = new DeployView(section, environment, release);
        return View(view);
    }

    // GET
    public async Task<IActionResult> Add(
        long sectionId,
        long environmentId,
        CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId, cancellationToken);
        if (section == null) throw new InvalidOperationException("Section doesn't exist: " + sectionId);
        var environment = section.GetEnvironment(environmentId);

        // set the value to the last of the most recent release.
        var value = environment.Releases.LastOrDefault()?.ModelValue.RootElement.ToString();
        var tokenSetName = environment.Releases.LastOrDefault()?.TokenSet?.TokenSetName;
        var tokenSets = (await _tokenSetQueries.GetTokenSetListAsync(cancellationToken))
            .Select(s => s.TokenSetName)
            .OrderBy(s => s)
            .ToList();

        var v = new EditReleaseView
        {
            EnvironmentName = environment.EnvironmentName,
            SectionName = section.SectionName,
            Schemas = section
                .Schemas
                .OrderByDescending(s => s.Version)
                .Select(s => new EditSchemaView(
                    "schema-" + s.Version.ToFullString().Replace(".", "-"),
                    s.Version.ToFullString(),
                    s.Schema.RootElement.ToString()))
                .ToList(),

            DefaultValue = value,
            DefaultTokenSetName = tokenSetName,
            TokenSetNames = tokenSets
        };

        return View(v);
    }

    [HttpPost]
    public async Task<CreateReleaseResponse> Create(CreateReleaseRequest request) =>
        await _mediator.Send(request);

    /// <summary>
    /// Execute a deployment.
    /// Handles the POST from Deploy.cshtml
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="environmentId"></param>
    /// <param name="releaseId"></param>
    /// <param name="notes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Deploy(long sectionId, long environmentId, long releaseId, string notes,
        CancellationToken cancellationToken)
    {
        var request = new DeployRequest
        {
            SectionId = sectionId,
            EnvironmentId = environmentId,
            ReleaseId = releaseId,
            Notes = notes
        };

        var response = await _mediator.Send(request, cancellationToken);
        return RedirectToAction("display", new
        {
            sectionId = sectionId,
            environmentId = environmentId,
            releaseId = releaseId
        });
    }

    public async Task<IActionResult> History(
        long? sectionId, 
        long? environmentId,
        CancellationToken cancellationToken)
    {
        if (sectionId == null || environmentId == null) throw new Exception("invalid input - temp exception");
        
        var section = await _sectionQueries.GetSectionAsync(sectionId.Value, cancellationToken);
        var env = section.GetEnvironment(environmentId.Value);
        var history = env
            .Releases
            .SelectMany(r =>
                r.Deployments.Select(h => new ReleaseHistoryDeploymentPair(r, h)))
            .OrderBy(r => r.Deployment.DeploymentDate)
            .ToList();
        var view = new ReleaseHistoryView(section, env, history);
        return View(view);
    }

    [HttpGet]
    public async Task<IActionResult> Display(ReleaseDisplayRequest request)
    {
        // temp
        if (request.SectionId == null || request.EnvironmentId == null || request.ReleaseId == null)
            throw new Exception("invalid input - temp exception");
        
        var section = await _sectionQueries.GetSectionAsync(request.SectionId!.Value);
        var env = section.GetEnvironment(request.EnvironmentId!.Value);
        var release = env.GetRelease(request.ReleaseId!.Value);
        return View(new ReleaseDisplayView(section, env, release));
    }
}

// TODO: the annotations aren't working... 
public class ReleaseDisplayRequest
{
    [Required] public long? SectionId { get; set; }
    [Required] public long? EnvironmentId { get; set; }
    [Required] public long? ReleaseId { get; set; }
}

public record ReleaseDisplayView(
    SectionDto Section,
    SectionEnvironmentDto SelectedEnvironment,
    SectionReleaseDto SelectedRelease);

public record ReleaseHistoryView(
    SectionDto SelectedSection,
    SectionEnvironmentDto SelectedEnvironment,
    List<ReleaseHistoryDeploymentPair> Deployments);

public record ReleaseHistoryDeploymentPair(
    SectionReleaseDto Release,
    SectionDeploymentDto Deployment);

public record DeployView(
    SectionDto Section,
    SectionEnvironmentDto Environment,
    SectionReleaseDto Release);

public class DeployRequest : IRequest<DeployResponse>
{
    public long SectionId { get; set; }
    public long EnvironmentId { get; set; }
    public long ReleaseId { get; set; }
    public string Notes { get; set; }
}

public record DeployResponse(long DeploymentId);