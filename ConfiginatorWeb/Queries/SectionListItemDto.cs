namespace ConfiginatorWeb.Queries;

public record SectionListItemDto(long SectionId, string Name, ISet<string> EnvironmentTypes);