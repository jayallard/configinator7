using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Versioning;

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
    
    public static IHtmlContent IsPreReleaseSchema(this IHtmlHelper helper, bool isPreRelease) =>
        helper.Raw(
            isPreRelease
                ? "<span class=\"badge rounded-pill bg-warning\">PreRelease</span>"
                : "");
    public static IHtmlContent SectionLink(this IHtmlHelper helper, string sectionName, long sectionId) =>
        helper.ActionLink(sectionName, "Display", "Section", new {sectionId});

    public static IHtmlContent ReleaseLink(this IHtmlHelper helper, long sectionId, long environmentId,
        long releaseId) =>
        helper.ActionLink(releaseId.ToString(), "DisplayRelease", "Release", new {sectionId, environmentId, releaseId});

    public static IHtmlContent VariableSetLink(this IHtmlHelper helper, string variableSetName) =>
        helper.ActionLink(variableSetName, "Index", "VariableSet", new {variableSetName});

    public static IHtmlContent SectionSchemaLink(this IHtmlHelper helper, long sectionId, string name) =>
        helper.ActionLink(name, "SchemaView", "Section", new {sectionId, name});
}