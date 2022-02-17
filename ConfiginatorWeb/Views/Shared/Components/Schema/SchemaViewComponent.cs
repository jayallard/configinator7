using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.Schema;

public class SchemaViewComponent : ViewComponent
{
    private readonly SchemaLoader _loader;
    private readonly EnvironmentValidationService _environmentValidationService;

    public SchemaViewComponent(SchemaLoader loader, EnvironmentValidationService environmentValidationService)
    {
        _loader = loader;
        _environmentValidationService = environmentValidationService;
    }

    public async Task<IViewComponentResult> InvokeAsync(SectionSchemaDto schema)
    {
        var promoteTo =
            _environmentValidationService.GetNextSchemaEnvironmentType(schema.EnvironmentTypes);
        var resolved = await _loader.ResolveSchemaAsync(schema.Schema);
        var dto = resolved.ToOutputDto();
        var view = (IViewComponentResult) View("Index", new SchemaIndexView(dto, schema, promoteTo));
        return view;
    }
}

public record SchemaIndexView(SchemaInfoDto Schema, SectionSchemaDto SectionSchema, string? PromoteToEnvironmentType);