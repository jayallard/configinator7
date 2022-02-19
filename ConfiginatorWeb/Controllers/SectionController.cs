using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using ConfiginatorWeb.Interactors.Section;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConfiginatorWeb.Controllers;

public class SectionController : Controller
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IMediator _mediator;

    private readonly ISchemaQueries _schemaQueries;

    // TODO: move to query handlers
    private readonly ISectionQueries _sectionQueries;

    public SectionController(
        ISectionQueries projections,
        IMediator mediator, EnvironmentValidationService environmentValidationService, ISchemaQueries schemaQueries)
    {
        _environmentValidationService = environmentValidationService;
        _schemaQueries = schemaQueries;
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

    public async Task<IActionResult> Display(long sectionId, CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId, cancellationToken);
        return View(new SectionIndexView(section));
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

public record SchemaView(SchemaInfoDto SchemaInfo, Dictionary<SchemaNameDto, SchemaDto> SchemaDtos, string? PromotableTo);

public record SectionIndexView(SectionDto Section);