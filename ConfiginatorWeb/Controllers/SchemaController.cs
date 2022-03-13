using System.ComponentModel.DataAnnotations;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using Allard.Json;
using ConfiginatorWeb.Interactors.Commands.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConfiginatorWeb.Controllers;

public class SchemaController : Controller
{
    private readonly EnvironmentDomainService _environmentDomainService;
    private readonly IMediator _mediator;
    private readonly INamespaceQueries _namespaceQueries;
    private readonly SchemaDomainService _schemaDomainService;
    private readonly SchemaLoader _schemaLoader;
    private readonly ISchemaQueries _schemaQueries;
    private readonly ISectionQueries _sectionQueries;

    public SchemaController(
        ISectionQueries sectionQueries,
        IMediator mediator,
        EnvironmentDomainService environmentDomainService,
        SchemaLoader schemaLoader,
        SchemaDomainService schemaDomainService,
        INamespaceQueries namespaceQueries, ISchemaQueries schemaQueries)
    {
        _sectionQueries = sectionQueries;
        _mediator = mediator;
        _environmentDomainService = environmentDomainService;
        _schemaLoader = schemaLoader;
        _schemaDomainService = schemaDomainService;
        _namespaceQueries = namespaceQueries;
        _schemaQueries = schemaQueries;
    }

    // GET
    public async Task<IActionResult> AddSchema(CancellationToken cancellationToken, long? sectionId)
    {
        var section = sectionId == null
            ? null
            : await _sectionQueries.GetSectionAsync(sectionId.Value, cancellationToken);
        var model = new AddSchemaViewModel
        {
            // language=json
            Schema =
                "{\n  \"type\": \"object\",\n \"required\": [ \"ExampleProperty1\" ],  \"properties\": {\n    \"ExampleProperty1\": {\n      \"type\": \"string\"\n    }\n  },\n  \"additionalProperties\": false\n}",
            SchemaName = null,

            // bug: shouldn't have to use the utility here. it shouldn't be wrong in the db.
            SelectedNamespace = section?.Namespace,
            SectionId = sectionId,
            SectionName = section?.SectionName
        };
        await SetViewData(model);
        return View(model);
    }

    private async Task SetViewData(AddSchemaViewModel model)
    {
        // if @namespace is specified, use it.
        // otherwise, get the list.
        if (model.IsForSection)
        {
            ViewData["ns"] = new List<SelectListItem>
                {new(model.SelectedNamespace, model.SelectedNamespace)};

            // hack - schemas for the section
            var allSchemas = (await _schemaQueries.GetSchemasListAsync())
                .Where(s => s.SectionId == model.SectionId)
                .ToList();
            ViewData["imports"] = allSchemas;
            return;
        }

        var ns = (await _namespaceQueries.GetNamespaces())
            .Select(n => new SelectListItem(n.NamespaceName, n.NamespaceName))
            .ToList();
        ViewData["ns"] = ns;
    }

    [HttpPost]
    public async Task<IActionResult> AddSchema(AddSchemaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await SetViewData(model);
            return View(model);
        }

        try
        {
            await _mediator.Send(new CreateSchemaRequest(model.SelectedNamespace, model.SchemaName, model.Schema,
                model.IsForSection ? new SectionId(model.SectionId.Value) : null));

            return model.IsForSection
                ? RedirectToAction("Display", "Section", new {model.SectionId})
                : RedirectToAction("Index", "Section");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            ModelState.AddModelError("ex", ex.Message);
            await SetViewData(model);
            return View(model);
        }
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
    public async Task<IActionResult> GetImportView(string schemaName)
    {
        var schemas = await _schemaDomainService.GetSchemasAsync(new[] {new SchemaName(schemaName)});
        var schema = schemas.Single();
        var import = new ImportSchemaView(
            schema.Namespace,
            schema.SchemaName.FullName,
            schema.Schema.RootElement.ToIndented());
        return Json(import);
    }

    [HttpGet]
    public async Task<IActionResult> PromoteIndex(string schemaName, CancellationToken cancellationToken)
    {
        // get the schema
        var schema = (await _schemaDomainService.GetSchemasAsync(new[] {new SchemaName(schemaName)}, cancellationToken))
            .Single();

        // figure out what we're trying to promote to
        var nextEnvironmentType = _environmentDomainService.GetNextSchemaEnvironmentType(schema.EnvironmentTypes,
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

public record PromoteModel(string? PromoteTo, bool IsOk, SchemaName SchemaName,
    List<PromoteModelReferenceStatus> ReferenceStatus);

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
    [Display(Name = "Namespace")]
    public string? SelectedNamespace { get; set; }

    public bool IsForSection => SectionId != null;
    public long? SectionId { get; set; }
    public string? SectionName { get; set; }
}

/// <summary>
///     Add schema page. A schema may be imported and tweaked, then saved as a new version.
///     This is the dto to provide the schema to copy.
/// </summary>
/// <param name="SchemaName"></param>
/// <param name="Namespace"></param>
/// <param name="Schema"></param>
public record ImportSchemaView(string Namespace, string SchemaName, string Schema);