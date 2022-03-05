using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using ConfiginatorWeb.Interactors.Commands.VariableSets;
using ConfiginatorWeb.Interactors.Queries.VariableSets;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ConfiginatorWeb.Controllers;

public class VariableSetController : Controller
{
    private readonly EnvironmentValidationService _environmentService;
    private readonly ILogger<VariableSetController> _logger;
    private readonly IMediator _mediator;

    public VariableSetController(
        IMediator mediator,
        EnvironmentValidationService environmentService, ILogger<VariableSetController> logger)
    {
        _environmentService = environmentService;
        _logger = logger;
        _mediator = Guards.HasValue(mediator, nameof(mediator));
    }

    // GET
    public async Task<IActionResult> Index(string variableSetName, CancellationToken cancellationToken)
    {
        var variableSet = await _mediator.Send(new VariableSetIndexQueryRequest(variableSetName), cancellationToken);
        return View(new EditVariableSetView(variableSet.VariableSet, variableSet.MermaidMarkup));
    }

    public async Task<IActionResult> EditValue(string variableSetName, string key, CancellationToken cancellationToken)
    {
        var variableSet = await _mediator.Send(new VariableSetIndexQueryRequest(variableSetName), cancellationToken);
        var variable =
            variableSet.VariableSet.Variables.ContainsKey(key)
                ? variableSet.VariableSet.Variables[key].Value
                : JToken.Parse("\"\"");

        return View(new EditValueView
        {
            VariableSetName = variableSetName,
            Key = key,
            SelectedVariable = variable,
            VariablesComposed = variableSet.VariableSet
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveValue(
        string variableSetName,
        string key,
        string value,
        CancellationToken cancellationToken)
    {
        var command = new SetVariableValueCommand(variableSetName, key, value);
        await _mediator.Send(command, cancellationToken);
        return RedirectToAction("index", new {variableSetName});
    }

    [HttpGet]
    public IActionResult AddVariableSet()
    {
        ViewData["EnvironmentTypes"] = _environmentService.EnvironmentTypeNames;
        return View(new AddVariableRequest());
    }

    [HttpGet]
    public IActionResult AddVariable(string variableSetName)
    {
        var model = new CreateVariableRequest {VariableSetName = variableSetName};
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddVariable(CreateVariableRequest request)
    {
        await _mediator.Send(request);
        return RedirectToAction("EditValue", new {request.VariableSetName, request.Key});
    }

    [HttpPost]
    public async Task<IActionResult> AddVariableSet(AddVariableRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewData["EnvironmentTypes"] = _environmentService.EnvironmentTypeNames;
            return View();
        }

        try
        {
            await _mediator.Send(request);
            return RedirectToAction("Index", new {request.VariableSetName});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddVariableSet");
            ViewData["EnvironmentTypes"] = _environmentService.EnvironmentTypeNames;
            ModelState.AddModelError(string.Empty, ex.Message);
            return View();
        }
    }
}

public record EditVariableSetView(VariableSetComposedDto VariableSet, string MermaidJsDiagram);

public class EditValueView
{
    public string VariableSetName { get; set; }

    public string Key { get; set; }

    public JToken SelectedVariable { get; set; }

    public VariableSetComposedDto VariablesComposed { get; set; }
}