using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Schema;
using ConfiginatorWeb.Queries;

namespace ConfiginatorWeb.Models;

public static class ExtensionMethods
{
    public static SchemaInfoDto ToOutputDto(this SchemaInfo schema)
    {
        return new SchemaInfoDto
        {
            Root = schema.Root.ToOutputDto(),
            References = schema.References.Select(ToOutputDto).ToList()
        };
    }

    public static SchemaDetailDto ToOutputDto(this SchemaDetail detail)
    {
        return new SchemaDetailDto
        {
            ReferencedBy = detail.ReferencedBy.Select(d => d.ToOutputDto()).ToList(),
            RefersTo = detail.RefersTo.Select(d => d.ToOutputDto()).ToList(),
            ResolvedSchema = detail.ResolvedSchema,
            SchemaSource = detail.SchemaSource,
            SchemaName = detail.SchemaName.FullName
        };
    }

    public static SchemaNameDto ToOutputDto(this SchemaName name)
    {
        return new SchemaNameDto(name.Name, name.Version, name.FullName);
    }
}