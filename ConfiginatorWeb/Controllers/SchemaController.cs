using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class SchemaController : Controller
{
    private readonly ISectionQueries _sectionQueries;

    public SchemaController(ISectionQueries sectionQueries)
    {
        _sectionQueries = sectionQueries;
    }

    // GET
    public async Task<IActionResult> Add(long sectionId, CancellationToken cancellationToken)
    {
        var section = await _sectionQueries.GetSectionAsync(sectionId, cancellationToken);
        ViewData["view"] = new AddSchemaViewModel(section);
        return View(new AddSchemaModel(sectionId, null, null));
    }

    [HttpPost]
    public IActionResult Add(AddSchemaModel model)
    {
        ModelState.AddModelError("blah", "didn't work");
        Console.WriteLine(model.Name);
        Console.WriteLine(model.Schema);
        return View("Add", model);
    }
}

public record AddSchemaModel(long SectionId, string? Name, string? Schema);

public record AddSchemaViewModel(SectionDto Section);