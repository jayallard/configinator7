using Configinator7.Core;
using Configinator7.Core.Model;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class TokenController : Controller
{
    private readonly SuperAggregate _aggregate;

    public TokenController(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    // GET
    public IActionResult Index(string tokenSetName)
    {
        var tokens = _aggregate.ResolveTokenSet(tokenSetName);
        if (tokens == null) throw new ArgumentException("make me a 404!");

        var v = new EditTokenSetView
        {
            Resolved = tokens
        };
        return View(v);
    }
}

public class EditTokenSetView
{
    public TokenSetResolved Resolved { get; set; }
}