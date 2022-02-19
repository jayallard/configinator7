using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.Schema;

public class SchemaViewComponent : ViewComponent
{
    private readonly SchemaLoader _loader;

    public SchemaViewComponent(SchemaLoader loader)
    {
        _loader = loader;
    }

    public async Task<IViewComponentResult> InvokeAsync(SchemaInfoDto schema)
    {
        // var resolved = await _loader.ResolveSchemaAsync(new SchemaName(schema.SchemaName.FullName), schema.Schema);
        // var dto = resolved.ToOutputDto();
        var view = (IViewComponentResult) View("Index", new SchemaIndexView(schema));
        return view;
    }
}

public record SchemaIndexView(SchemaInfoDto Schema);