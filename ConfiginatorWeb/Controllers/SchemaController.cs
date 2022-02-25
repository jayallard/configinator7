﻿using System.ComponentModel.DataAnnotations;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Interactors.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConfiginatorWeb.Controllers;

public class SchemaController : Controller
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly IMediator _mediator;
    private readonly SchemaLoader _schemaLoader;
    private readonly ISectionQueries _sectionQueries;
    private readonly SchemaDomainService _schemaDomainService;
    private readonly INamespaceQueries _namespaceQueries;

    public SchemaController(
        ISectionQueries sectionQueries,
        IMediator mediator,
        EnvironmentValidationService environmentValidationService,
        SchemaLoader schemaLoader,
        SchemaDomainService schemaDomainService, 
        INamespaceQueries namespaceQueries)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _environmentValidationService = environmentValidationService;
        _schemaLoader = schemaLoader;
        _schemaDomainService = schemaDomainService;
        _namespaceQueries = namespaceQueries;
    }

    // GET
    public async Task<IActionResult> AddSchema(CancellationToken cancellationToken)
    {
        var ns = (await _namespaceQueries.GetNamespaces())
            .Select(n => new SelectListItem(n.NamespaceName, n.NamespaceId.ToString()));
        var model = new AddSchemaViewModel
        {
            // language=json
            Schema = "{\n  \"type\": \"object\",\n  \"properties\": {\n    \"ExampleProperty1\": {\n      \"type\": \"string\"\n    }\n  },\n  \"additionalProperties\": false\n}",
            SchemaName = null,
            SelectedNamespace = null,
            Namespaces = ns
        };
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddSchema(AddSchemaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        await _mediator.Send(new CreateSchemaRequest( model.SelectedNamespace, model.SchemaName, model.Schema));
        return RedirectToAction("Index", "Section");
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

public class AddSchemaViewModel
{
    [Required]
    [Display(Name = "Schema Name", Description = "name/version. IE:  my-schema/1.0.0")]
    public string? SchemaName { get; set; }

    [Required]
    [Display(Name = "JSON Schema")]
    public string? Schema { get; set; }

    [Required]
    [Display(Name="Namespace")]
    public string? SelectedNamespace { get; set; }
    public IEnumerable<SelectListItem>? Namespaces { get; set; }

}