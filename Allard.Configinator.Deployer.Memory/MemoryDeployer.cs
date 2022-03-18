using System.Text.Json;
using Allard.Configinator.Deployer.Abstractions;

namespace Allard.Configinator.Deployer.Memory;

public class MemoryDeployer : IDeployer
{
    private readonly IConfigurationProvider _configurationProvider;
    private readonly MemoryConfigurationStore _store;

    public MemoryDeployer(
        IConfigurationProvider configurationProvider,
        MemoryConfigurationStore store)
    {
        _configurationProvider = configurationProvider;
        _store = store;
    }

    public async Task<DeployerDescription> GetDescriptionAsync(DeployRequest request,
        CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationProvider.GetConfigurationAsync(request);
        var sanitized = Sanitize(configuration);
        return new DeployerDescription
        {
            Name = nameof(MemoryDeployer),
            Description =
                "A deployer test implementation. Values are stored in dictionaries. There is one dictionary per simulated \"region\". Different environments are assigned to different regions via configuration.",
            ViewableConfiguration = sanitized
        };
    }

    /// <summary>
    ///     Deploy to the memory dictionary.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DeployResult> DeployAsync(DeployRequest request, CancellationToken cancellationToken = default)
    {
        const string source = nameof(MemoryDeployer);
        var result = new DeployResult();
        try
        {
            // get the config
            result.AddInformation(source, "Start.GetConfiguration", "Getting configuration");
            var configuration = await _configurationProvider.GetConfigurationAsync(request);
            result.AddInformation(source, "End.GetConfiguration", "Getting configuration complete");

            // set the value
            result.AddInformation(source, "Start.SaveValue", "Getting region");
            _store.SetValue(configuration.RegionName, configuration.Path, request.ResolvedValue);
            result.AddInformation(source, "End.SaveValue", "Setting value complete");

            // done
            return result;
        }
        catch (Exception ex)
        {
            // sad face. :(
            result.AddError("DeployAsync", "Boom", "Exception Occurred", ex.ToString());
            return result;
        }
    }

    /// <summary>
    ///     Return a configuration document with only safe displayable values.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static JsonDocument Sanitize(MemoryDeploymentConfiguration configuration)
    {
        return JsonSerializer.SerializeToDocument(new
        {
            configuration.RegionName,
            configuration.Path,
            UserName = "<redacted>",
            Password = "<redacted>"
        });
    }
}