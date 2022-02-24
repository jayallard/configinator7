using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
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
    private readonly SchemaDomainService _schemaDomainService;

    public SchemaController(
        ISectionQueries sectionQueries,
        IMediator mediator,
        EnvironmentValidationService environmentValidationService,
        SchemaLoader schemaLoader,
        SchemaDomainService schemaDomainService)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _environmentValidationService = environmentValidationService;
        _schemaLoader = schemaLoader;
        _schemaDomainService = schemaDomainService;
    }

    // GET
    public async Task<IActionResult> AddSchema(long? sectionId, CancellationToken cancellationToken)
    {
        if (sectionId == null)
        {
            return View(new AddSchemaModel(null, null));
        }
        
        var section = await _sectionQueries.GetSectionAsync(sectionId.Value, cancellationToken);
        return View(new AddSchemaModel(sectionId.Value, section.SectionName));
    }

    [HttpPost]
    public async Task<IActionResult> AddSchema(long? sectionId, string @namespace, string schemaName, string schemaText)
    {
        await _mediator.Send(new CreateSchemaRequest(sectionId, @namespace, schemaName, schemaText));
        return Json(new {ok = "then"});
    }
    
    [HttpPost]
    public async Task<IActionResult> Promote(string schemaName, string targetEnvironmentType)
    {
        var result = await _mediator.Send(new PromoteSchemaRequest(schemaName, targetEnvironmentType));
        return View("PromotionResult", result);
    }

    [HttpGet]
    public IActionResult SchemaView(string schemaName)
    {
        return View(new SchemaName(schemaName));
    }

    [HttpGet]
    public async Task<IActionResult> PromoteIndex(string schemaName, CancellationToken cancellationToken)
    {
        // get the schema
        var schema = (await _schemaDomainService.GetSchemasAsync(new[] {new SchemaName(schemaName)}, cancellationToken))
            .Single();
        
        // figure out what we're trying to promote to
        var nextEnvironmentType = _environmentValidationService.GetNextSchemaEnvironmentType(schema.EnvironmentTypes,
            schema.SchemaName.Version);

        // resolve the schema
        var resolved = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema, cancellationToken);

        // get all the schemas related to the one we're trying to promote
        var allSchemas = (await _schemaDomainService.GetSchemasAsync(resolved.AllNames(), cancellationToken))
            .ToDictionary(s => s.SchemaName, s => s);
        
        // find any child schemas that need to be promoted before this one
        var status = resolved
            .References
            .Select(r => new PromoteModelReferenceStatus(
                r.SchemaName.ToOutputDto(),
                allSchemas[r.SchemaName].EnvironmentTypes.Contains(nextEnvironmentType),
                allSchemas[r.SchemaName].EnvironmentTypes.ToHashSet()))
            .ToList();
        
        // it's ok to promote if all child schemas exist in the environment type
        var isOk = status.All(s => s.IsOk);
        
        // we have all we need. Proceed.
        var model = new PromoteModel(nextEnvironmentType, isOk, schema.SchemaName, status);
        return View(model);
    }
}

public record PromoteModel(string? PromoteTo, bool IsOk, SchemaName SchemaName, List<PromoteModelReferenceStatus> ReferenceStatus);

public record PromoteModelReferenceStatus(SchemaNameDto SchemaName, bool IsOk, ISet<string> EnvironmentTypes);

public record AddSchemaModel(long? SectionId, string? SectionName)
{
    public bool IsGlobal() => SectionId == null;
}
