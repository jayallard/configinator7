using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, Section> TemporaryExposure => _sections;

    #endregion

    // key = Configuration Section Name
    private readonly Dictionary<string, Section> _sections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, JToken>> _tokenSets = new(StringComparer.OrdinalIgnoreCase);

    private void Play(IEvent evt)
    {
        switch (evt)
        {
            case SectionCreatedEvent(var sectionName, var path, var schema, var tokenSetName):
            {
                var id = new SectionId(sectionName);
                var section = new Section
                {
                    Path = path,
                    Id = id,
                    TokenSetName = tokenSetName
                };
                
                if (schema != null)
                {
                    section.Schemas.Add(schema);
                }

                _sections.Add(sectionName, section);
                break;
            }
            case EnvironmentAddedToSectionEvent(var environmentName, var sectionName):
            {
                var section = GetSection(sectionName);
                section.Environments.Add(new ConfigurationEnvironment
                {
                    EnvironmentId = new ConfigurationEnvironmentId(environmentName),
                });
                break;
            }
            case SchemaAddedToSection(var sectionName, var schema):
            {
                // make a copy of the configuration section and increment the version.
                // add the configuratioi section to the history list.
                GetSection(sectionName).Schemas.Add(schema);
                break;
            }
            case ReleaseCreatedEvent resolved:
            {
                var (_, schema) = GetSchema(resolved.SectionName, resolved.Version);
                var (_, environment) = GetEnvironment(resolved.SectionName, resolved.EnvironmentName);
                var release = new Release(
                    resolved.ReleaseId,
                    resolved.ModelValue,
                    resolved.ResolvedValue,
                    resolved.Tokens,
                    resolved.Version,
                    resolved.EventDate,
                    null);
                environment.Releases.Add(release);
                break;
            }
            case TokenSetCreatedEvent created:
            {
                _tokenSets[created.TokenSetName] = created.Tokens;
                break;
            }
            case ReleaseDeployed deployed:
            {
                var (_, _, release) = GetRelease(deployed.SectionName, deployed.EnvironmentName, deployed.ReleaseId);
                release.Deployments.Add(new Deployment(deployed.EventDate, DeploymentAction.Set, string.Empty));
                release.IsDeployed = true;
                break;
            }
            case ReleaseUndeployed undeployed:
            {
                var (_, _, release) = GetRelease(undeployed.SectionName, undeployed.EnvironmentName, undeployed.ReleaseId);
                release.Deployments.Add(new Deployment(undeployed.EventDate, DeploymentAction.Removed, undeployed.Reason));
                release.IsDeployed = false;
                break;
            }
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public SectionId CreateSection(
        string sectionName,
        ConfigurationSchema? schema,
        string? path,
        string? tokenSetName)
    {
        EnsureSectionDoesntExist(sectionName);
        EnsureTokenSetExists(tokenSetName);
        Play(new SectionCreatedEvent(sectionName, path, schema, tokenSetName));
        return _sections[sectionName].Id;
    }

    public void AddSchema(string sectionName, ConfigurationSchema schema)
    {
        var section = GetSection(sectionName);
        if (section.Schemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        Play(new SchemaAddedToSection(sectionName, schema));
    }

    private ICollection<ValidationError> Validate(JObject value, JsonSchema schema) => schema.Validate(value);

    public void CreateTokenSet(string name, Dictionary<string, JToken> tokens)
    {
        EnsureTokenSetDoesntExist(name);
        tokens = tokens.ToDictionary(t => t.Key, t => t.Value?.DeepClone());
        Play(new TokenSetCreatedEvent(name, tokens));
    }

    public async Task CreateReleaseAsync(
        string sectionName,
        string environmentName,
        SemanticVersion schemaVersion,
        JObject value)
    {
        var (_, schema) = GetSchema(sectionName, schemaVersion);
        var resolved = await JsonUtility.ResolveAsync(value, new Dictionary<string, JToken>());
        var results = Validate(resolved, schema.Schema);
        if (results.Any())
        {
            throw new SchemaValidationFailedException(results);
        }

        GetEnvironment(sectionName, environmentName);
        var releaseId = (_sections
            .Values
            .SelectMany(s => s.Environments.SelectMany(h => h.Releases))
            .Max(r => r.ReleaseId as long?) ?? 0) + 1;
        Play(new ReleaseCreatedEvent(releaseId, sectionName, environmentName, schemaVersion, value, resolved, null));
    }

    private (Section Section, ConfigurationSchema Schemaa) GetSchema(string sectionName, SemanticVersion version)
    {
        var section = GetSection(sectionName);
        var schema = section.Schemas.SingleOrDefault(s => s.Version == version);
        if (schema == null)
            throw new InvalidOperationException(
                $"Schema doesnt' exist. Configuration Section Name={sectionName}, Version={version.ToFullString()}");
        return new(section, schema);
    }

    public void AddEnvironment(string sectionName, string environmentName)
    {
        EnsureEnvironmentDoesntExist(sectionName, environmentName);
        GetSection(sectionName);
        Play(new EnvironmentAddedToSectionEvent(environmentName, sectionName));
    }

    public void Deploy(string sectionName, string environmentName, long releaseId)
    {
        var (_, environment, _) = GetRelease(sectionName, environmentName, releaseId);
        
        // if a release is already deployed, then set it to undeployed
        var released = environment.Releases.SingleOrDefault(r => r.IsDeployed && r.ReleaseId != releaseId);
        if (released != null)
        {
            Play(new ReleaseUndeployed(sectionName, environmentName, released.ReleaseId, $"Overwritten by Release #{releaseId}"));
        }

        // set the new release to deployed
        Play(new ReleaseDeployed(sectionName, environmentName, releaseId));
    }

    private Section GetSection(string sectionName)
    {
        EnsureSectionExists(sectionName);
        return _sections[sectionName];
    }

    private (Section Section, ConfigurationEnvironment Environment) GetEnvironment(string sectionName, string environmentName)
    {
        var section = GetSection(sectionName);
        var environment = section
            .Environments
            .Single(h => h.EnvironmentId.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
        return new(section, environment);
    }

    private (Section Section, ConfigurationEnvironment Environment, Release Release) GetRelease(string sectionName, string environmentName,
        long releaseId)
    {
        var (section, environment) = GetEnvironment(sectionName, environmentName);
        var release = environment.Releases.SingleOrDefault(r => r.ReleaseId == releaseId);
        if (release == null)
        {
            throw new InvalidOperationException("Release does not exist");
        }

        return new ValueTuple<Section, ConfigurationEnvironment, Release>(section, environment, release);
    }
    
    private void EnsureSectionExists(string sectionName)
    {
        if (!_sections.ContainsKey(sectionName))
            throw new InvalidOperationException("Configuration Section does not exist: " + sectionName);
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

    private void EnsureSectionDoesntExist(string sectionName)
    {
        if (_sections.ContainsKey(sectionName))
            throw new InvalidOperationException("Configuration Section already exists: " + sectionName);
    }

    private void EnsureEnvironmentDoesntExist(string sectionName, string environmentName)
    {
        var section = GetSection(sectionName);
        if (section.Environments.Select(h => h.EnvironmentId.Name).Contains(environmentName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The environment already exists: " + environmentName);
        }
    }
}