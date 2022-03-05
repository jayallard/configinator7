using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Core.Specifications.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.Schema;

public class SchemaViewComponent : ViewComponent
{
    private readonly EnvironmentValidationService _environmentValidationService;
    private readonly SchemaDomainService _schemaDomainService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;

    public SchemaViewComponent(
        SchemaLoader schemaLoader,
        IUnitOfWork unitOfWork,
        EnvironmentValidationService environmentValidationService,
        SchemaDomainService schemaDomainService)
    {
        _schemaLoader = schemaLoader;
        _unitOfWork = unitOfWork;
        _environmentValidationService = environmentValidationService;
        _schemaDomainService = schemaDomainService;
    }

    public async Task<IViewComponentResult> InvokeAsync(SchemaName schemaName)
    {
        Guards.HasValue(schemaName, nameof(SchemaName));
        var schema = await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(schemaName));
        var resolved = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema);
        var aggregates = await _schemaDomainService.GetSchemasAsync(resolved.AllNames());
        var dtos =
            aggregates
                // todo: move this
                .Select(s => new SchemaDto
                {
                    Schema = s.Schema,

                    // HACK - blocking
                    SectionName = s.SectionId == null
                        ? null
                        : _unitOfWork.Sections.GetAsync(s.SectionId).Result.SectionName,
                    EnvironmentTypes = s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
                    SchemaName = s.SchemaName.ToOutputDto(),
                    SectionId = s.SectionId?.Id,
                    SchemaId = s.Id.Id,
                    Namespace = s.Namespace,
                    PromoteTo = _environmentValidationService.GetNextSchemaEnvironmentType(s.EnvironmentTypes,
                        s.SchemaName.Version)
                })
                .ToDictionary(s => s.SchemaName.FullName, s => s);


        // var resolved = await _loader.ResolveSchemaAsync(new SchemaName(schema.SchemaName.FullName), schema.Schema);
        // var dto = resolved.ToOutputDto();
        var mermaid = MermaidUtility.FlowChartForSchemas(resolved.References.Union(new[] {resolved.Root}),
            resolved.Root.SchemaName);
        var view = (IViewComponentResult) View("Index", new SchemaIndexView(resolved.ToOutputDto(), dtos, mermaid));
        return view;
    }
}

public record SchemaIndexView(SchemaInfoDto Schema, Dictionary<string, SchemaDto> Dtos, string MermaidJs);