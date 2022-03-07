namespace ConfiginatorWeb.Queries;

public record VariableSetListItemDto(
    long VariableSetId,
    string Namespace, 
    string VariableSetName, 
    string? EnvironmentType,
    string? BaseVariableSetName,
    string? Mermaid);