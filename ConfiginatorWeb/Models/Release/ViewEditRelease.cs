using Configinator7.Core.Model;

namespace ConfiginatorWeb.Models.Release;

public class ViewEditRelease
{
    public string SecretName { get; set; }
    public string HabitatName { get; set; }
    public List<ConfigurationSchema> Schemas { get; set; }
}
