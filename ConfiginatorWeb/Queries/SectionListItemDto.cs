namespace ConfiginatorWeb.Queries;

public record SectionListItemDto(long SectionId, string Name, string OrganizationPath, ISet<string> EnvironmentTypes);