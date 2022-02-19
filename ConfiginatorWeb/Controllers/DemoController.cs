using Allard.Configinator.Deployer.Memory;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

/// <summary>
///     All things useful for development and demo, but not part of configinator.
/// </summary>
public class DemoController : Controller
{
    private readonly MemoryConfigurationStore _store;

    public DemoController(MemoryConfigurationStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult MemoryConfigurationStore()
    {
        return View(_store.Values);
    }
}