using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Models;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.Schema;

public class SchemaViewComponent : ViewComponent
{
    private readonly SchemaLoader _loader;

    public SchemaViewComponent(SchemaLoader loader)
    {
        _loader = loader;
    }

    public async Task<IViewComponentResult> InvokeAsync(SectionSchemaDto schema)
    {
        var resolved = (await _loader.ResolveSchemaAsync(schema.Schema)).ToOutputDto();
        var view = (IViewComponentResult) View("Index", new SchemaIndexView(resolved));
        return view;
    }
}

public record SchemaIndexView(SchemaInfoDto Schema);
