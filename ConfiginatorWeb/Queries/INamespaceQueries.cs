namespace ConfiginatorWeb.Queries;

public interface INamespaceQueries
{
    Task<List<NamespaceDto>> GetNamespaces();
}