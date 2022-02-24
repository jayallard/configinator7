using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class HtmlExtensionMethods
{
    public static IHtmlContent IsOutOfDateBadge(this IHtmlHelper helper, bool isOutOfDate)
    {
        return helper.Raw(
            isOutOfDate
                ? "<span class=\"badge rounded-pill bg-warning\">Out of Date</span>"
                : string.Empty);
    }

    public static IHtmlContent IsDeployedBadge(this IHtmlHelper helper, bool isDeployed, string? tail = null)
    {
        return helper.Raw(
            isDeployed
                ? "<span class=\"badge rounded-pill bg-success\">Deployed</span>" + tail
                : "");
    }

    public static IHtmlContent IsPreReleaseSchema(this IHtmlHelper helper, bool isPreRelease)
    {
        return helper.Raw(
            isPreRelease
                ? "<span class=\"badge rounded-pill bg-warning\">PreRelease</span>"
                : "");
    }

    public static IHtmlContent SectionLink(this IHtmlHelper helper, string @namespace, string sectionName, long sectionId)
    {
        return helper.ActionLink( @namespace  + "/" + sectionName, "Display", "Section", new {sectionId});
    }

    public static IHtmlContent ReleaseLink(this IHtmlHelper helper, long sectionId, long environmentId,
        long releaseId)
    {
        return helper.ActionLink(releaseId.ToString(), "DisplayRelease", "Release",
            new {sectionId, environmentId, releaseId});
    }

    public static IHtmlContent VariableSetLink(this IHtmlHelper helper, string variableSetName)
    {
        return helper.ActionLink(variableSetName, "Index", "VariableSet", new {variableSetName});
    }

    public static IHtmlContent SchemaLink(this IHtmlHelper helper, string schemaName)
    {
        return helper.ActionLink(schemaName, "SchemaView", "Schema", new {schemaName});
    }
}