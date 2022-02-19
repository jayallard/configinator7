using System.Text.Json;
using ConfiginatorWeb.Queries;
using NJsonSchema;

namespace ConfiginatorWeb.Models;

public class SchemaInfoDto
{
    public SchemaDetailDto Root { get; set; }
    public List<SchemaDetailDto> References { get; set; }
}

public class SchemaDetailDto
{
    public List<string> EnvironmentTypes { get; set; }
    public string SchemaName { get; set; }
    public List<SchemaNameDto> ReferencedBy { get; set; }
    public List<SchemaNameDto> RefersTo { get; set; }
    public JsonSchema ResolvedSchema { get; set; }
    public JsonDocument SchemaSource { get; set; }
}