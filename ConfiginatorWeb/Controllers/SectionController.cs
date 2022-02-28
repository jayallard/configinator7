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
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IMediator _mediator;
    private readonly ISectionQueries _sectionQueries;

    public SectionController(
        ISectionQueries projections,
        IMediator mediator,
        EnvironmentValidationService environmentValidationService, ISchemaQueries schemaQueries,
        SectionDomainService sectionDomainService)
    {
        _environmentValidationService = environmentValidationService;
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
        if (sectionId == null) throw new ArgumentException(nameof(sectionId));
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
        if (model.SelectedEnvironments != null && model.SelectedEnvironments.Count > 0)
        {
            await _mediator.Send(new AddEnvironmentsToSectionRequest(model.SectionId, model.SelectedEnvironments));
        }

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
        var environmentTypes = _environmentValidationService
            .EnvironmentTypeNames
            .Where(e => section.EnvironmentTypes.Contains(e))
            .Select(et =>
            {
                var environments = _environmentValidationService
                    .EnvironmentNames
                    .Where(e => e.EnvironmentType.Equals(et, StringComparison.OrdinalIgnoreCase))
                    .Select(e =>
                        new EnvironmentItemViewData(e.EnvironmentName, environmentsInUse.Contains(e.EnvironmentName)))
                    .ToList();
                return new EnvironmentTypeItemViewData(et, environments);
            })
            .ToList();

        ViewData["environmentTypes"] = environmentTypes;

        // -------------------------------------
        // for promotion section
        // -------------------------------------
        var nextEnvironmentType = _environmentValidationService.GetNextSectionEnvironmentType(section.EnvironmentTypes);
        ViewData["PromoteTo"] = nextEnvironmentType;

        var canAdd = environmentTypes.Any(et => et.EnvironmentItems.Any(e => !e.IsAlreadyInUse));
        return View(new AddEnvironmentViewModel(sectionId, new List<string>(), canAdd));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string name,
        string @namespace,
        List<string> selectedEnvironments)
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

public record EnvironmentTypesViewData(List<EnvironmentTypeItemViewData> EnvironmentTypes);

public record EnvironmentTypeItemViewData(string EnvironmentType, List<EnvironmentItemViewData> EnvironmentItems);

public record EnvironmentItemViewData(string EnvironmentName, bool IsAlreadyInUse);