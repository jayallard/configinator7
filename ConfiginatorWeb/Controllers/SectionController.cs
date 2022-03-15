using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Interactors.Commands.Section;
using ConfiginatorWeb.Interactors.Queries.Section;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConfiginatorWeb.Controllers;

public class SectionController : Controller
{
    private readonly EnvironmentDomainService _environmentDomainService;
    private readonly IMediator _mediator;
    private readonly ISectionQueries _sectionQueries;

    public SectionController(
        ISectionQueries projections,
        IMediator mediator,
        EnvironmentDomainService environmentDomainService)
    {
        _environmentDomainService = Guards.HasValue(environmentDomainService, nameof(environmentDomainService));
        _mediator = Guards.HasValue(mediator, nameof(mediator));
        _sectionQueries = Guards.HasValue(projections, nameof(projections));
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var view = await _mediator.Send(new IndexRequest());
        return View(view);
    }

    public async Task<IActionResult> Create()
    {
        var view = await _mediator.Send(new AddSectionIndexQueryRequest());
        return View(view);
    }

    public async Task<IActionResult> Display(long? sectionId, CancellationToken cancellationToken)
    {
        if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));
        var section = await _sectionQueries.GetSectionAsync(sectionId.Value, cancellationToken);
        return View(new SectionIndexView(section));
    }

    [HttpPost]
    public async Task<IActionResult> Promote(long sectionId, string environmentType)
    {
        var request = new PromoteSectionRequest(sectionId, environmentType);
        await _mediator.Send(request);
        return RedirectToAction("Display", new {sectionId});
    }

    [HttpPost]
    public async Task<IActionResult> AddEnvironment(AddEnvironmentViewModel model)
    {
        if (model.SelectedEnvironments is {Count: > 0})
            await _mediator.Send(new AddEnvironmentsToSectionRequest(model.SectionId, model.SelectedEnvironments));

        return RedirectToAction("Display", new {sectionId = model.SectionId});
    }

    [HttpGet]
    public async Task<IActionResult> AddEnvironment(long sectionId, CancellationToken cancellationToken = default)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId, cancellationToken);
        var environmentsInUse = section
            .Environments
            .Select(e => e.EnvironmentName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // get a list of environment types
        // per environment type, get the environments
        // per environment, indicate if it's already in use by the section
        var environmentTypes = _environmentDomainService
            .EnvironmentTypes
            .Where(et => section.EnvironmentTypes.Contains(et.EnvironmentTypeName, StringComparer.OrdinalIgnoreCase))
            .Select(et =>
            {
                var env = et.AllowedEnvironments.Select(e =>
                    new EnvironmentItemViewData(e, environmentsInUse.Contains(e)));
                return new EnvironmentTypeItemViewData(et.EnvironmentTypeName, env.ToList());
            })
            .ToList();
        ViewData["environmentTypes"] = environmentTypes;

        // -------------------------------------
        // for promotion section
        // -------------------------------------
        var nextEnvironmentType = _environmentDomainService.GetNextEnvironmentType(section.EnvironmentTypes);
        ViewData["PromoteTo"] = nextEnvironmentType?.EnvironmentTypeName;

        var canAdd = environmentTypes
            .SelectMany(et => et.EnvironmentItems)
            .Any(e => !e.IsAlreadyInUse);
        return View(new AddEnvironmentViewModel(sectionId, new List<string>(), canAdd));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string name,
        string @namespace)
    {
        if (!ModelState.IsValid) return View();
        try
        {
            var request = new CreateSectionAppRequest
            {
                Name = name,
                Namespace = @namespace
            };

            var result = await _mediator.Send(request);
            return RedirectToAction("Display", new {result.SectionId});
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
    }
}

public record SectionIndexView(SectionDto Section);

public record AddEnvironmentViewModel(long SectionId, List<string>? SelectedEnvironments, bool CanAdd);

public record EnvironmentTypeItemViewData(string EnvironmentType, List<EnvironmentItemViewData> EnvironmentItems);

public record EnvironmentItemViewData(string EnvironmentName, bool IsAlreadyInUse);