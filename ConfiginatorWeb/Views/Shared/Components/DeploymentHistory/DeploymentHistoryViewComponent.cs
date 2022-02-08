using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.DeploymentHistory;

public class DeploymentHistoryViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(SectionReleaseDto release)
    {
        var view = (IViewComponentResult) View("Index", release);
        return Task.FromResult(view);
    }
}