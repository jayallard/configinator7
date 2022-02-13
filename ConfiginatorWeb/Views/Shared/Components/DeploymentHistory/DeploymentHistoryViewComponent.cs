using ConfiginatorWeb.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConfiginatorWeb.Views.Shared.Components.DeploymentHistory;

public class DeploymentHistoryViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(
        SectionDto section,
        SectionEnvironmentDto environment,
        SectionReleaseDto release)
    {
        var view = (IViewComponentResult) View("Index", new DeploymentHistoryView(section, environment, release));
        return Task.FromResult(view);
    }
}

public record DeploymentHistoryView(SectionDto Section, SectionEnvironmentDto Environment, SectionReleaseDto Release);