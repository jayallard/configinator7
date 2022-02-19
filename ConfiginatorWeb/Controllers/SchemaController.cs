using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications.Schema;
using ConfiginatorWeb.Interactors.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class SchemaController : Controller
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IMediator _mediator;
    private readonly SchemaLoader _schemaLoader;
    private readonly ISectionQueries _sectionQueries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SchemaDomainService _schemaDomainService;

    public SchemaController(ISectionQueries sectionQueries, IMediator mediator,
        EnvironmentValidationService environmentValidationService, IUnitOfWork unitOfWork, SchemaLoader schemaLoader,
        SchemaDomainService schemaDomainService)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _environmentValidationService = environmentValidationService;
        _unitOfWork = unitOfWork;
        _schemaLoader = schemaLoader;
        _schemaDomainService = schemaDomainService;
    }

    // GET
    public async Task<IActionResult> AddSectionSchema(long sectionId, CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId, cancellationToken);
        ViewData["view"] = new AddSchemaViewModel(section);
        return View(new AddSchemaModel(sectionId, null, null));
    }

    [HttpPost]
    public async Task<IActionResult> AddSectionSchema(AddSchemaModel model)
    {
        await _mediator.Send(new CreateSectionSchemaRequest(model.SectionId, model.SchemaName, model.Schema));
        return Json(new {ok = "then"});
    }

    [HttpPost]
    public async Task<IActionResult> PromoteSectionSchema(string schemaName, string targetEnvironmentType)
    {
        // todo: messed up
        await _mediator.Send(new PromoteSchemaRequest(schemaName, targetEnvironmentType));
        return RedirectToAction("SchemaView", "Schema", new {schemaName});
    }

    public async Task<IActionResult> SchemaView(string schemaName, CancellationToken cancellationToken)
    {
        return View(new SchemaName(schemaName));
    }
}

public record AddSchemaModel(long SectionId, string SchemaName, string Schema);

public record AddSchemaViewModel(SectionDto Section);