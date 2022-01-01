using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, ConfigurationSection> TemporaryExposure => _configurationSections;

    #endregion

    // key = Configuration Section Name
    private readonly Dictionary<string, ConfigurationSection> _configurationSections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, JToken>> _tokenSets = new(StringComparer.OrdinalIgnoreCase);

    private void Play(IEvent evt)
    {
        switch (evt)
        {
            case ConfigurationSectionCreatedEvent(var configurationSectionName, var path, var schema, var tokenSetName):
            {
                var id = new ConfigurationSectionId(configurationSectionName);
                var configurationSection = new ConfigurationSection
                {
                    Path = path,
                    Id = id,
                    TokenSetName = tokenSetName
                };
                
                if (schema != null)
                {
                    configurationSection.Schemas.Add(schema);
                }

                _configurationSections.Add(configurationSectionName, configurationSection);
                break;
            }
            case HabitatAddedToConfigurationSectionEvent(var habitatName, var configurationSectionName):
            {
                var configurationSection = GetConfigurationSection(configurationSectionName);
                configurationSection.Habitats.Add(new Habitat
                {
                    HabitatId = new HabitatId(habitatName),
                });
                break;
            }
            case SchemaAddedToConfigurationSection(var configurationSectionName, var schema):
            {
                // make a copy of the configuration section and increment the version.
                // add the configuratioi section to the history list.
                GetConfigurationSection(configurationSectionName).Schemas.Add(schema);
                break;
            }
            case ReleaseCreatedEvent resolved:
            {
                var (_, schema) = GetSchema(resolved.ConfigurationSectionName, resolved.Version);
                var (_, habitat) = GetHabitat(resolved.ConfigurationSectionName, resolved.HabitatName);
                var release = new Release(
                    resolved.ReleaseId,
                    resolved.ModelValue,
                    resolved.ResolvedValue,
                    resolved.Tokens,
                    resolved.Version,
                    resolved.EventDate,
                    null);
                habitat.Releases.Add(release);
                break;
            }
            case TokenSetCreatedEvent created:
            {
                _tokenSets[created.TokenSetName] = created.Tokens;
                break;
            }
            case ReleaseDeployed deployed:
            {
                var (_, _, release) = GetRelease(deployed.ConfigurationSectionName, deployed.HabitatName, deployed.ReleaseId);
                release.Deployments.Add(new Deployment(deployed.EventDate, DeploymentAction.Set, string.Empty));
                release.IsDeployed = true;
                break;
            }
            case ReleaseUndeployed undeployed:
            {
                var (_, _, release) = GetRelease(undeployed.ConfigurationSectionName, undeployed.HabitatName, undeployed.ReleaseId);
                release.Deployments.Add(new Deployment(undeployed.EventDate, DeploymentAction.Removed, undeployed.Reason));
                release.IsDeployed = false;
                break;
            }
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public ConfigurationSectionId CreateConfigurationSection(
        string configurationSectionName,
        ConfigurationSchema? schema,
        string? path,
        string? tokenSetName)
    {
        EnsureConfigurationSectionDoesntExist(configurationSectionName);
        EnsureTokenSetExists(tokenSetName);
        Play(new ConfigurationSectionCreatedEvent(configurationSectionName, path, schema, tokenSetName));
        return _configurationSections[configurationSectionName].Id;
    }

    public void AddSchema(string configurationSectionName, ConfigurationSchema schema)
    {
        var configurationSection = GetConfigurationSection(configurationSectionName);
        if (configurationSection.Schemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        Play(new SchemaAddedToConfigurationSection(configurationSectionName, schema));
    }

    private ICollection<ValidationError> Validate(JObject value, JsonSchema schema) => schema.Validate(value);

    public void CreateTokenSet(string name, Dictionary<string, JToken> tokens)
    {
        EnsureTokenSetDoesntExist(name);
        tokens = tokens.ToDictionary(t => t.Key, t => t.Value?.DeepClone());
        Play(new TokenSetCreatedEvent(name, tokens));
    }

    public async Task CreateReleaseAsync(
        string configurationSectionName,
        string habitatName,
        SemanticVersion schemaVersion,
        JObject value)
    {
        var (_, schema) = GetSchema(configurationSectionName, schemaVersion);
        var resolved = await JsonUtility.ResolveAsync(value, new Dictionary<string, JToken>());
        var results = Validate(resolved, schema.Schema);
        if (results.Any())
        {
            throw new SchemaValidationFailedException(results);
        }

        var habitat = GetHabitat(configurationSectionName, habitatName);
        
        var releaseId = (_configurationSections
            .Values
            .SelectMany(s => s.Habitats.SelectMany(h => h.Releases))
            .Max(r => r.ReleaseId as long?) ?? 0) + 1;
        Play(new ReleaseCreatedEvent(releaseId, configurationSectionName, habitatName, schemaVersion, value, resolved, null));
    }

    private (ConfigurationSection ConfigurationSection, ConfigurationSchema Schemaa) GetSchema(string configurationSectionName, SemanticVersion version)
    {
        var configurationSection = GetConfigurationSection(configurationSectionName);
        var schema = configurationSection.Schemas.SingleOrDefault(s => s.Version == version);
        if (schema == null)
            throw new InvalidOperationException(
                $"Schema doesnt' exist. Configuration Section Name={configurationSectionName}, Version={version.ToFullString()}");
        return new(configurationSection, schema);
    }

    public void AddHabitat(string configurationSectionName, string habitatName)
    {
        EnsureHabitatDoesntExist(configurationSectionName, habitatName);
        GetConfigurationSection(configurationSectionName);
        Play(new HabitatAddedToConfigurationSectionEvent(habitatName, configurationSectionName));
    }

    public void Deploy(string configurationSectioName, string habitatName, long releaseId)
    {
        var (_, habitat, _) = GetRelease(configurationSectioName, habitatName, releaseId);
        
        // if a release is already deployed, then set it to undeployed
        var released = habitat.Releases.SingleOrDefault(r => r.IsDeployed && r.ReleaseId != releaseId);
        if (released != null)
        {
            Play(new ReleaseUndeployed(configurationSectioName, habitatName, released.ReleaseId, $"Overwritten by Release #{releaseId}"));
        }

        // set the new release to deployed
        Play(new ReleaseDeployed(configurationSectioName, habitatName, releaseId));
    }

    private ConfigurationSection GetConfigurationSection(string configurationSectionName)
    {
        EnureConfigurationSectionExists(configurationSectionName);
        return _configurationSections[configurationSectionName];
    }

    private (ConfigurationSection ConfigurationSection, Habitat Habitat) GetHabitat(string configurationSectionName, string habitatName)
    {
        var configurationSection = GetConfigurationSection(configurationSectionName);
        var habitat = configurationSection
            .Habitats
            .Single(h => h.HabitatId.Name.Equals(habitatName, StringComparison.OrdinalIgnoreCase));
        return new(configurationSection, habitat);
    }

    private (ConfigurationSection ConfigurationSection, Habitat Habitat, Release Release) GetRelease(string configurationSectionName, string habitatName,
        long releaseId)
    {
        var (configurationSection, habitat) = GetHabitat(configurationSectionName, habitatName);
        var release = habitat.Releases.SingleOrDefault(r => r.ReleaseId == releaseId);
        if (release == null)
        {
            throw new InvalidOperationException("Release does not exist");
        }

        return new ValueTuple<ConfigurationSection, Habitat, Release>(configurationSection, habitat, release);
    }
    
    private void EnureConfigurationSectionExists(string configurationSectionName)
    {
        if (!_configurationSections.ContainsKey(configurationSectionName))
            throw new InvalidOperationException("Configuration Section does not exist: " + configurationSectionName);
    }

    private void EnsureTokenSetDoesntExist(string tokenSetName)
    {
        if (_tokenSets.ContainsKey(tokenSetName))
            throw new InvalidOperationException("Token set already exists: " + tokenSetName);
    }

    private void EnsureTokenSetExists(string? tokenSetName)
    {
        if (tokenSetName == null) return;
        if (!_tokenSets.ContainsKey(tokenSetName))
            throw new InvalidOperationException("Token set doesn't exist: " + tokenSetName);
    }

    private void EnsureConfigurationSectionDoesntExist(string configurationSectionName)
    {
        if (_configurationSections.ContainsKey(configurationSectionName))
            throw new InvalidOperationException("Configuration Section already exists: " + configurationSectionName);
    }

    private void EnsureHabitatDoesntExist(string configurationSectionName, string habitatName)
    {
        var configurationSection = GetConfigurationSection(configurationSectionName);
        if (configurationSection.Habitats.Select(h => h.HabitatId.Name).Contains(habitatName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The habitat already exists: " + habitatName);
        }
    }
}