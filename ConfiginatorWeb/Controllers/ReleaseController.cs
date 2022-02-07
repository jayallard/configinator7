using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Interactors;
using ConfiginatorWeb.Models;
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
    private readonly SchemaLoader _schemaLoader;

    public ReleaseController(
        ISectionQueries sectionQueries,
        ITokenSetQueries tokenSetQueries,
        IMediator mediator, SchemaLoader schemaLoader)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _schemaLoader = schemaLoader;
        _tokenSetQueries = tokenSetQueries;
    }

    // GET
    public async Task<IActionResult> Add(
        string sectionName,
        string environmentName,
        CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionName, cancellationToken);
        if (section == null) throw new InvalidOperationException("Section doesn't exist: " + sectionName);
        var environment = section.GetEnvironment(environmentName);

        // set the value to the last of the most recent release.
        var value = environment.Releases.LastOrDefault()?.ModelValue.RootElement.ToString();
        var tokenSetName = environment.Releases.LastOrDefault()?.TokenSet?.TokenSetName;
        var tokenSets = (await _tokenSetQueries.GetTokenSetListAsync(cancellationToken))
            .Select(s => s.TokenSetName)
            .OrderBy(s => s)
            .ToList();

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

    [HttpPost]
    public async Task<ReleaseDeployResponse>
        Deploy(ReleaseDeployRequest request, CancellationToken cancellationToken) =>
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
        var schemaDetails = await _schemaLoader.ResolveSchemaAsync(schema.Schema);
        return View(new ReleaseDisplayView(section, env, release, schemaDetails.ToOutputDto()));
    }
}

public record ReleaseDisplayView(
    SectionDto Section,
    SectionEnvironmentDto SelectedEnvironment,
    SectionReleaseDto SelectedRelease,
    SchemaInfoDto SelectedSchema);

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