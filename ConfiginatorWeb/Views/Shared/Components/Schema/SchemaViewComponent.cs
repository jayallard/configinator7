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
    private readonly EnvironmentDomainService _environmentDomainService;
    private readonly SchemaDomainService _schemaDomainService;
    private readonly SchemaLoader _schemaLoader;
    private readonly IUnitOfWork _unitOfWork;

    public SchemaViewComponent(
        SchemaLoader schemaLoader,
        IUnitOfWork unitOfWork,
        EnvironmentDomainService environmentDomainService,
        SchemaDomainService schemaDomainService)
    {
        _schemaLoader = schemaLoader;
        _unitOfWork = unitOfWork;
        _environmentDomainService = environmentDomainService;
        _schemaDomainService = schemaDomainService;
    }

    public async Task<IViewComponentResult> InvokeAsync(SchemaName schemaName)
    {
        // TODO: this only shows the schema and it's references.
        // if the schema is referred to by other schemas, it doesn't show
        // them
        Guards.HasValue(schemaName, nameof(SchemaName));
        var schema = await _unitOfWork.Schemas.FindOneAsync(SchemaNameIs.Is(schemaName));
        var resolved = await _schemaLoader.ResolveSchemaAsync(schema.SchemaName, schema.Schema);
        var aggregates = await _schemaDomainService.GetSchemasAsync(resolved.AllNames());
        var dtosTasks =
            aggregates
                // todo: move this
                .Select(async s => new SchemaDto
                {
                    Schema = s.Schema,
                    SectionName = s.SectionId == null
                        ? null
                        : (await _unitOfWork.Sections.GetAsync(s.SectionId)).SectionName,
                    EnvironmentTypes = s.EnvironmentTypes.ToHashSet(StringComparer.OrdinalIgnoreCase),
                    SchemaName = s.SchemaName.ToOutputDto(),
                    SectionId = s.SectionId?.Id,
                    SchemaId = s.Id.Id,
                    Namespace = s.Namespace,
                    PromoteTo = _environmentDomainService.GetNextSchemaEnvironmentType(s.EnvironmentTypes,
                        s.SchemaName.Version)
                })
                .ToArray();
        await Task.WhenAll(dtosTasks);
        var dtos = dtosTasks.ToDictionary(s => s.Result.SchemaName.FullName, s => s.Result);
        var mermaid = MermaidUtility.FlowChartForSchemas(resolved.References.Union(new[] {resolved.Root}),
            resolved.Root.SchemaName);
        var view = (IViewComponentResult) View("Index", new SchemaIndexView(resolved.ToOutputDto(), dtos, mermaid));
        return view;
    }
}

public record SchemaIndexView(SchemaInfoDto Schema, Dictionary<string, SchemaDto> Dtos, string MermaidJs);