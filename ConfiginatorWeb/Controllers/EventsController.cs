using Allard.Configinator.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Controllers;

public class EventsController : Controller
{
    public IActionResult Index()
    {
        // currently, they're all event 
        return View(UnitOfWorkMemory.AllEventsTempHack);
    }
}