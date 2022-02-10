using System.Collections.Concurrent;
using System.Text.Json;
using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public class MemoryDeployer : IDeployer
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JsonDocument>> _configuration = new();
    private readonly IConfigurationProvider _configurationProvider;

    public MemoryDeployer(IConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }

    public async Task<DeployerDescription> GetDescriptionAsync(DeployRequest request, CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationProvider.GetConfigurationAsync(request);
        var sanitized = Sanitize(configuration);
        return new DeployerDescription
        {
            Name = nameof(MemoryDeployer),
            Description = "A deployer test implementation. Values are stored in dictionaries. There is one dictionary per simulated \"region\". Different environments are assigned to different regions via configuration.",
            ViewableConfiguration = sanitized
        };
    }

    /// <summary>
    /// Return a configuration document with only safe displayable values.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static JsonDocument Sanitize(MemoryDeploymentConfiguration configuration) =>
        JsonSerializer.SerializeToDocument(new
        {
            configuration.RegionName,
            configuration.Path,
            UserName = "<redacted>",
            Password = "<redacted>"
        });

    /// <summary>
    /// Deploy to the memory dictionary.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeployResult> DeployAsync(DeployRequest request, CancellationToken cancellationToken = default)
    {
        var result = new DeployResult();
        try
        {
            // get the config
            result.AddInformation("DeployAsync", "Start.GetConfiguration", "Getting configuration");
            var configuration = await _configurationProvider.GetConfigurationAsync(request);
            result.AddInformation("DeployAsync", "End.GetConfiguration", "Getting configuration complete");

            // connect to the region - simulator. it's just a dictionary
            result.AddInformation("DeployAsync", "Start.ConnectRegion", "Getting region");
            var region = _configuration.GetOrAdd(configuration.RegionName,
                r => new ConcurrentDictionary<string, JsonDocument>());
            result.AddInformation("DeployAsync", "End.ConnectRegion", "Getting region complete");

            // set the value
            result.AddInformation("DeployAsync", "End.SaveValue", "Setting the value");
            region[configuration.Path] = request.ResolvedValue;
            result.AddInformation("DeployAsync", "End.SaveValue", "Setting value complete");

            // done
            return result;
        }
        catch (Exception ex)
        {
            // sad face. :(
            result.AddError("DeployAsync", "Boom", "Exception Occurred", ex);
            return result;
        }
    }
}