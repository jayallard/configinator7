using System.Text.Json;
using NJsonSchema;

namespace ConfiginatorWeb.Models;

public class SchemaInfoDto
{
    public SchemaDetailDto Root { get; set; }
    public List<SchemaDetailDto> References { get; set; }
}

public class SchemaDetailDto
{
    public string Name { get; set; }
    public List<string> ReferencedBy { get; set; }
    public List<string> RefersTo { get; set; }
    public JsonSchema ResolvedSchema { get; set; }
    public JsonDocument SchemaSource { get; set; }
}