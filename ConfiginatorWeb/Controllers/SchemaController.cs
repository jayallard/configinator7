using ConfiginatorWeb.Interactors.Schema;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class SchemaController : Controller
{
    private readonly ISectionQueries _sectionQueries;
    private readonly IMediator _mediator;

    public SchemaController(ISectionQueries sectionQueries, IMediator mediator)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
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
    public async Task<IActionResult> PromoteSectionSchema(long? sectionId, string schemaName, string targetEnvironmentType)
    {
        if (sectionId == null) throw new InvalidOperationException("section id is required");
        await _mediator.Send(new PromoteSectionSchemaRequest(sectionId.Value, schemaName, targetEnvironmentType));
        return RedirectToAction("SchemaView", "Section", new {sectionId = sectionId.Value, name = schemaName});
    }
}

public record AddSchemaModel(long SectionId, string SchemaName, string Schema);

public record AddSchemaViewModel(SectionDto Section);