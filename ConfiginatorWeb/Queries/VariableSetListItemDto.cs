namespace ConfiginatorWeb.Queries;

public record VariableSetListItemDto(string Namespace, string VariableSetName, string? EnvironmentType,
    string? BaseVariableSetName,
    string? Mermaid);