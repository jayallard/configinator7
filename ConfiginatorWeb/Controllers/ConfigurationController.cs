using Allard.Configinator.Core;
using ConfiginatorWeb.Models.Configuration;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ConfigurationController : Controller
{
    // TODO: move to query handlers
    private readonly ISectionQueries _sectionQueries;
    private readonly IVariableSetQueries _variableSetQueries;
    private readonly IGlobalSchemaQueries _globalSchemaQueries;

    public ConfigurationController(ISectionQueries projections, IVariableSetQueries variableSetQueries, IGlobalSchemaQueries globalSchemaQueries)
    {
        _globalSchemaQueries = globalSchemaQueries;
        _sectionQueries = Guards.NotDefault(projections, nameof(projections));
        _variableSetQueries = Guards.NotDefault(variableSetQueries, nameof(variableSetQueries));
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var sections = _sectionQueries.GetSectionsListAsync();
        var variableSets = _variableSetQueries.GetVariableSetListAsync();
        var schemas = _globalSchemaQueries.GetGlobalSchemasListAsync();

        var t = await variableSets;
        var roots = t.Where(t => t.BaseVariableSetName == null);
        
        var view = new IndexView(await sections, t, await schemas);
        return View(view);
    }

    public IActionResult Create()
    {
        return View();
    }

    public async Task<IActionResult> Display(long sectionId)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId);
        return View(section);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateConfiguration config)
    {
        /*
        if (!ModelState.IsValid) return View();

        try
        {
            _aggregate.CreateSection(config.Name, null, config.Path, null);
        }
        catch (JsonReaderException ex)
        {
            ModelState.AddModelError("schema", ex.Message);
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("error", ex.Message);
            return View();
        }*/

        return RedirectToAction("Index");
    }
}

public record IndexView(List<SectionListItemDto> Sections, List<VariableSetListItemDto> VariableSets, List<GlobalSchemaListItemDto> GlobalSchemas);
public record GlobalSchemaListItemDto(long GlobalSchemaId, string Name, string? Description);