namespace ConfiginatorWeb.Queries;

public record SectionListItemDto(long SectionId, string Namespace, string Name, ISet<string> EnvironmentTypes);