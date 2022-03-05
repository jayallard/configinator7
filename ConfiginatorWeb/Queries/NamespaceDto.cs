namespace ConfiginatorWeb.Queries;

public class NamespaceDto
{
    public long NamespaceId { get; set; }
    public string NamespaceName { get; set; }
    public List<NamespaceSchemaDto> Schemas { get; set; }
    public List<NamespaceSectionDto> Sections { get; set; }
    public List<NamespaceVariableSetDto> VariableSets { get; set; }
}

public record NamespaceSchemaDto(long SchemaId, SchemaNameDto SchemaName, long? SectionId, string? SectionName,
    List<string> EnvironmentTypes)
{
    public bool IsForSection => SectionId != null;
}

public record NamespaceSectionDto(long SectionId, string SectionName,
    List<NamespaceSectionEnvironmentDto> Environments);

public record NamespaceVariableSetDto(long VariableSetId, string VariableSetName, string EnvironmentType);

public record NamespaceSectionEnvironmentDto(string EnvironmentName, bool IsOutOfDate, bool IsDeployed);