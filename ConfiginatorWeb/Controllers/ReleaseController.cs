using Configinator7.Core.Model;
using ConfiginatorWeb.Models.Release;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class ReleaseController : Controller
{
    private readonly SuperAggregate _aggregate;

    public ReleaseController(SuperAggregate aggregate)
    {
        _aggregate = aggregate;
    }

    // GET
    public IActionResult Edit(string secretName, string habitatName)
    {
        var v = new ViewEditRelease
        {
            HabitatName = habitatName,
            SecretName = secretName,
            Schemas = _aggregate.TemporarySecretExposure[secretName].Schemas
        };
        return View(v);
    }
}