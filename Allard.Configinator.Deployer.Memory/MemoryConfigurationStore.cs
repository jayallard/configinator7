using System.Collections.Concurrent;
using System.Text.Json;

namespace Allard.Configinator.Deployer.Memory;

public class MemoryConfigurationStore
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JsonDocument>> _configuration = new();

    public void SetValue(string regionName, string key, JsonDocument value)
    {
        var region = _configuration.GetOrAdd(regionName, r => new ConcurrentDictionary<string, JsonDocument>());
        region[key] = value;
    }

    // demo... not concerned about immutability. at least not yet... maybe some day it will evolve to a proper demo,
    // but today is not that day. today is hack day.
    public ConcurrentDictionary<string, ConcurrentDictionary<string, JsonDocument>> Values => _configuration;
}