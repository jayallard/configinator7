using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Interactors.Section;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Versioning;

namespace ConfiginatorWeb.Controllers;

public class SectionController : Controller
{
    // TODO: move to query handlers
    private readonly ISectionQueries _sectionQueries;
    private readonly IMediator _mediator;
    private readonly EnvironmentValidationService _environmentValidationService;

    public SectionController(
        ISectionQueries projections,
        IMediator mediator, EnvironmentValidationService environmentValidationService)
    {
        _environmentValidationService = environmentValidationService;
        _mediator = Guards.HasValue(mediator, nameof(mediator));
        _sectionQueries = Guards.HasValue(projections, nameof(projections));
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var view = await _mediator.Send(new ConfigurationIndexRequest());
        return View(view);
    }

    public async Task<IActionResult> Create()
    {
        var view = await _mediator.Send(new GetAddSectionViewModel());
        return View(view);
    }

    public async Task<IActionResult> Display(long sectionId)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId);
        return View(section);
    }

    public async Task<IActionResult> SchemaView(long sectionId, string name)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId);
        var schema = section.GetSchema(name);
        return View(new SchemaView(section, schema,
            _environmentValidationService.GetNextSchemaEnvironmentType(schema.EnvironmentTypes, schema.Name.Version)));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string name,
        string path,
        List<string> selectedEnvironments)
    {
        if (!ModelState.IsValid) return View();
        try
        {
            var request = new CreateSectionAppRequest
            {
                Name = name,
                EnvironmentNames = selectedEnvironments
            };
            var result = await _mediator.Send(request);
            return RedirectToAction("Display", new {SectionId = result.SectionId});
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

public record SchemaView(SectionDto Section, SectionSchemaDto Schema, string? PromotableTo);