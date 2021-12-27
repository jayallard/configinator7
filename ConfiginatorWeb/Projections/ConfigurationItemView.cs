using Configinator7.Core.Model;

namespace ConfiginatorWeb.Projections;

public class ConfigurationItemView
{
    public SecretId SecretId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
}