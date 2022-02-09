using Allard.Configinator.Core;
using ConfiginatorWeb.Models.Configuration;
using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ConfigurationController : Controller
{
    // TODO: move to query handlers
    private readonly ISectionQueries _sectionQueries;
    private readonly ITokenSetQueries _tokenSetQueries;
    private readonly IGlobalSchemaQueries _globalSchemaQueries;

    public ConfigurationController(ISectionQueries projections, ITokenSetQueries tokenSetQueries, IGlobalSchemaQueries globalSchemaQueries)
    {
        _globalSchemaQueries = globalSchemaQueries;
        _sectionQueries = Guards.NotDefault(projections, nameof(projections));
        _tokenSetQueries = Guards.NotDefault(tokenSetQueries, nameof(tokenSetQueries));
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var sections = _sectionQueries.GetSectionsListAsync();
        var tokensSets = _tokenSetQueries.GetTokenSetListAsync();
        var schemas = _globalSchemaQueries.GetGlobalSchemasListAsync();

        var t = await tokensSets;
        var roots = t.Where(t => t.BaseTokenSetName == null);
        
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

public record IndexView(List<SectionListItemDto> Sections, List<TokenSetListItemDto> TokenSets, List<GlobalSchemaListItemDto> GlobalSchemas);
public record GlobalSchemaListItemDto(long GlobalSchemaId, string Name, string? Description);