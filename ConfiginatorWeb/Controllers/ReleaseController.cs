using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using Allard.Json;
using ConfiginatorWeb.Interactors.Commands.Release;
using ConfiginatorWeb.Models.Release;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ReleaseController : Controller
{
    private readonly IMediator _mediator;
    private readonly SchemaLoader _schemaLoader;
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;
    private readonly SectionDomainService _sectionDomainService;

    public ReleaseController(
        IVariableSetQueries variableSetQueries,
        IMediator mediator,
        SchemaLoader schemaLoader,
        ISectionQueries sectionQueries,
        SectionDomainService sectionDomainService)
    {
        _mediator = mediator;
        _schemaLoader = schemaLoader;
        _sectionQueries = sectionQueries;
        _sectionDomainService = sectionDomainService;
        _variableSetQueries = variableSetQueries;
    }

    [HttpGet]
    public async Task<string> GetSchemaSample(string schemaName)
    {
        var resolved = await _schemaLoader.ResolveSchemaAsync(new SchemaName(schemaName));
        return resolved.Root.ResolvedSchema.ToSampleJson().ToString();
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
        long? sectionId,
        long? environmentId,
        CancellationToken cancellationToken)
    {
        // temp
        if (sectionId == null || environmentId == null) throw new Exception("bad input");

        var section = await _sectionQueries.GetSectionAsync(sectionId.Value, cancellationToken);
        if (section == null) throw new InvalidOperationException("Section doesn't exist: " + sectionId);
        var environment = section.GetEnvironment(environmentId.Value);

        // set the value to the last of the most recent release.
        var value = environment.Releases.LastOrDefault()?.ModelValue.RootElement.ToIndented();
        var variableSetName = environment.Releases.LastOrDefault()?.VariableSet?.VariableSetName;
        var schemaName = environment.Releases.LastOrDefault()?.Schema?.SchemaName?.FullName;
        var variableSets = (await _variableSetQueries.GetVariableSetListAsync(cancellationToken))
            .Where(s => s.EnvironmentType.Equals(environment.EnvironmentType, StringComparison.OrdinalIgnoreCase))
            .Where(s => NamespaceUtility.IsSelfOrAscendant(s.Namespace, section.Namespace))
            .Select(s => new EditSchemaVariableView(s.VariableSetName, s.VariableSetId))
            .OrderBy(s => s.VariableSetName)
            .ToList();

        var v = new EditReleaseView
        {
            Namespace = section.Namespace,
            EnvironmentName = environment.EnvironmentName,
            EnvironmentId = environment.EnvironmentId,
            SectionName = section.SectionName,
            SectionId = section.SectionId,
            Schemas = section
                .Schemas
                .Where(s => s.EnvironmentTypes.Contains(environment.EnvironmentType, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(s => s.SchemaName.FullName)
                .Select(s => new EditSchemaView(
                    s.SchemaName,
                    s.SchemaId,
                    s.Schema.RootElement.ToString()))
                .ToList(),

            DefaultValue = value,
            DefaultVariableSetName = variableSetName,
            DefaultSchemaName = schemaName,
            VariableSet = variableSets
        };

        return View(v);
    }

    [HttpPost]
    public async Task<CreateReleaseResponse> Create(CreateReleaseRequest request)
    {
        return await _mediator.Send(request);
    }

    /// <summary>
    ///     Execute a deployment.
    ///     Handles the POST from Deploy.cshtml
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
        var request = new HttpDeployRequest
        {
            SectionId = sectionId,
            EnvironmentId = environmentId,
            ReleaseId = releaseId,
            Notes = notes
        };

        var response = await _mediator.Send(request, cancellationToken);
        return RedirectToAction("DisplayDeployment", new
        {
            sectionId,
            environmentId,
            releaseId,
            response.DeploymentId
        });
    }

    [HttpGet]
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
    public async Task<IActionResult> DisplayRelease(ReleaseDisplayRequest request)
    {
        // temp
        if (request.SectionId == null || request.EnvironmentId == null || request.ReleaseId == null)
            throw new Exception("invalid input - temp exception");

        var section = await _sectionQueries.GetSectionAsync(request.SectionId!.Value);
        var env = section.GetEnvironment(request.EnvironmentId!.Value);
        var release = env.GetRelease(request.ReleaseId!.Value);
        return View(new ReleaseDisplayView(section, env, release));
    }

    [HttpGet]
    public async Task<IActionResult> DisplayDeployment(DeploymentDisplayRequest request)
    {
        // temp
        if (request.SectionId == null || request.EnvironmentId == null || request.ReleaseId == null ||
            request.DeploymentId == null)
            throw new Exception("invalid input - temp exception");
        var section = await _sectionQueries.GetSectionAsync(request.SectionId.Value);
        var env = section.GetEnvironment(request.EnvironmentId.Value);
        var release = env.GetRelease(request.ReleaseId!.Value);
        var deployment = release.GetDeployment(request.DeploymentId.Value);
        var view = (object) new DisplayDeploymentView(section, env, release, deployment);
        return View(view);
    }

    [HttpPost]
    public async Task<PreviewResponse> Preview(PreviewRequest request)
    {
        try
        {
            var resolvedValue = await _sectionDomainService.ResolveValue(
                new SectionId(request.SectionId),
                new EnvironmentId(request.EnvironmentId),
                request.VariableSetId == null
                    ? null
                    : new VariableSetId(request.VariableSetId.Value),
                new SchemaId(request.SchemaId),
                JsonDocument.Parse(request.Value));
            return new PreviewResponse(resolvedValue.RootElement.ToIndented(), Array.Empty<string>());
        }
        catch (SchemaValidationFailedException ex)
        {
            return new PreviewResponse(ex.InvalidJson.RootElement.ToIndented(), ex.Errors.Select(e => e.ToString()));
        }
        catch (Exception ex)
        {
            // TODO - capture the json and specific variable errors
            return new PreviewResponse("{ \"TODO\" : {}}", new[] {ex.Message});
        }
    }
}

public record DisplayDeploymentView(
    SectionDto Section,
    SectionEnvironmentDto Environment,
    SectionReleaseDto Release,
    SectionDeploymentDto Deployment);

// TODO: the annotations aren't working... 
public class ReleaseDisplayRequest
{
    [Required] public long? SectionId { get; set; }
    [Required] public long? EnvironmentId { get; set; }
    [Required] public long? ReleaseId { get; set; }
}

public class DeploymentDisplayRequest
{
    [Required] public long? SectionId { get; set; }
    [Required] public long? EnvironmentId { get; set; }
    [Required] public long? ReleaseId { get; set; }
    [Required] public long? DeploymentId { get; set; }
}

public record ReleaseDisplayView(
    SectionDto Section,
    SectionEnvironmentDto Environment,
    SectionReleaseDto Release);

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

public class HttpDeployRequest : IRequest<DeployResponse>
{
    public long SectionId { get; set; }
    public long EnvironmentId { get; set; }
    public long ReleaseId { get; set; }
    public string Notes { get; set; }
}

public record DeployResponse(long DeploymentId);

public record PreviewRequest(long SectionId, long EnvironmentId, long SchemaId, long? VariableSetId, string Value);

public record PreviewResponse(string Json, IEnumerable<string> Errors);