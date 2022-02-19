using System.Collections.Concurrent;
using System.Text.Json;

namespace Allard.Configinator.Deployer.Memory;

public class MemoryConfigurationStore
{
    // demo... not concerned about immutability. at least not yet... maybe some day it will evolve to a proper demo,
    // but today is not that day. today is hack day.
    public ConcurrentDictionary<string, ConcurrentDictionary<string, JsonDocument>> Values { get; } = new();

    public void SetValue(string regionName, string key, JsonDocument value)
    {
        var region = Values.GetOrAdd(regionName, r => new ConcurrentDictionary<string, JsonDocument>());
        region[key] = value;
    }
}