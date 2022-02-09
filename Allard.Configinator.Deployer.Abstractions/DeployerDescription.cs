using System.Text.Json;

namespace Allard.Configinator.Deployer.Abstractions;

public class DeployerDescription
{
    public string Name { get; set; }
    public string Description { get; set; }
    public JsonDocument ViewableConfiguration { get; set; }
}