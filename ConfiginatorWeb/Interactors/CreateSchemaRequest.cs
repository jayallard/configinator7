using Allard.Configinator.Core.Model;

namespace ConfiginatorWeb.Interactors;

public record CreateSchemaRequest(string SectionName, SchemaEntity Schema);