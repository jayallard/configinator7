using Allard.Configinator.Core;
using ConfiginatorWeb.Interactors.Section;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ConfiginatorWeb.Controllers;

public class SectionController : Controller
{
    // TODO: move to query handlers
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;
    private readonly IGlobalSchemaQueries _globalSchemaQueries;
    private readonly IMediator _mediator;

    public SectionController(ISectionQueries projections, IVariableSetQueries variableSetQueries,
        IGlobalSchemaQueries globalSchemaQueries, IMediator mediator)
    {
        _mediator = Guards.NotDefault(mediator, nameof(mediator));
        _globalSchemaQueries = Guards.NotDefault(globalSchemaQueries, nameof(globalSchemaQueries));
        _sectionQueries = Guards.NotDefault(projections, nameof(projections));
        _variableSetQueries = Guards.NotDefault(variableSetQueries, nameof(variableSetQueries));
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
                OrganizationPath = path,
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