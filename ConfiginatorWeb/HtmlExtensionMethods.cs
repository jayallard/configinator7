
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class HtmlExtensionMethods
{
    public static IHtmlContent IsOutOfDateBadge(this IHtmlHelper helper, bool isOutOfDate) =>
        helper.Raw(
            isOutOfDate
                ? "<span class=\"badge rounded-pill bg-warning\">Out of Date</span>"
                : string.Empty);

    public static IHtmlContent IsDeployedBadge(this IHtmlHelper helper, bool isDeployed, string? tail = null) =>
        helper.Raw(
            isDeployed
                ? "<span class=\"badge rounded-pill bg-success\">Deployed</span>" + tail
                : "");
}