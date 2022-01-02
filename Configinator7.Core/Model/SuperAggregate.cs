using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, Section> TemporaryExposureSections => _sections;
    public Dictionary<string, TokenSet> TemporaryExposureTokenSets => _tokenSets;

    #endregion

    // key = Configuration Section Name
    private readonly Dictionary<string, Section> _sections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TokenSet> _tokenSets = new(StringComparer.OrdinalIgnoreCase);

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
                var (_, environment) = GetEnvironment(resolved.SectionName, resolved.EnvironmentName);
                var release = new Release(
                    resolved.ReleaseId,
                    resolved.ModelValue,
                    resolved.ResolvedValue,
                    resolved.Tokens,
                    resolved.Schema,
                    resolved.EventDate,
                    null);
                environment.Releases.Add(release);
                break;
            }
            case TokenSetCreatedEvent(var name, var tokens, var baseTokenSetName):
            {
                _tokenSets[name] = new TokenSet
                {
                    TokenSetName = name,
                    Tokens = tokens,
                    Base = baseTokenSetName
                };
                break;
            }
            case ReleaseDeployed deployed:
            {
                var (_, environment, release) =
                    GetRelease(deployed.SectionName, deployed.EnvironmentName, deployed.ReleaseId);


                // set all of the deployments to NOT DEPLOYED
                // TODO: this is business logic - shouldn't be here. this is wrong.
                foreach (var d in environment.Releases.SelectMany(r => r.Deployments))
                {
                    d.IsDeployed = false;
                }

                // set this deployment to DEPLOYED
                release.Deployments.Add(new Deployment(deployed.EventDate, DeploymentAction.Deployed, string.Empty)
                    {IsDeployed = true});
                release.IsDeployed = true;
                break;
            }
            case ReleaseUndeployed undeployed:
            {
                var (_, _, release) =
                    GetRelease(undeployed.SectionName, undeployed.EnvironmentName, undeployed.ReleaseId);
                release.Deployments.Add(new Deployment(undeployed.EventDate, DeploymentAction.Removed,
                    undeployed.Reason));
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
        EnsureTokenSetExistsIfNotNull(tokenSetName);
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

    public void AddTokenSet(string name, Dictionary<string, JToken> tokens, string baseTokenSet = null)
    {
        EnsureTokenSetExistsIfNotNull(baseTokenSet);
        EnsureTokenSetDoesntExist(name);
        tokens = tokens.ToDictionary(t => t.Key, t => t.Value.DeepClone(), StringComparer.OrdinalIgnoreCase);
        Play(new TokenSetCreatedEvent(name, tokens, baseTokenSet));
    }

    public async Task CreateReleaseAsync(
        string sectionName,
        string environmentName,
        string? tokenSetName,
        SemanticVersion schemaVersion,
        JObject value)
    {
        var (_, schema) = GetSchema(sectionName, schemaVersion);
        
        // get the tokens, if there are any.
        // if not, use an empty dictionary.
        var resolvedTokens = ResolveTokenSet(tokenSetName);
        var tokens = resolvedTokens == null
            ? new Dictionary<string, JToken>()
            : resolvedTokens
                .Tokens
                .Values
                .ToDictionary(t => t.Name, t => t.Value, StringComparer.OrdinalIgnoreCase);
        
        var resolved = await JsonUtility.ResolveAsync(value, tokens);
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


        var ts = tokenSetName == null
            ? null
            : new TokenSetResolver(_tokenSets.Values).Resolve(tokenSetName);
        Play(new ReleaseCreatedEvent(releaseId, sectionName, environmentName, schema, value, resolved, resolvedTokens));
    }

    public TokenSetResolved? ResolveTokenSet(string? tokenSetName)
    {
        if (tokenSetName == null) return null;
        return new TokenSetResolver(_tokenSets.Values).Resolve(tokenSetName);
    }

    private TokenSet GetTokenSet(string name)
    {
        EnsureTokenSetExistsIfNotNull(name);
        return _tokenSets[name];
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
            Play(new ReleaseUndeployed(sectionName, environmentName, released.ReleaseId,
                $"Overwritten by Release #{releaseId}"));
        }

        // set the new release to deployed
        Play(new ReleaseDeployed(sectionName, environmentName, releaseId));
    }


    private Section GetSection(string sectionName)
    {
        EnsureSectionExists(sectionName);
        return _sections[sectionName];
    }

    private (Section Section, ConfigurationEnvironment Environment) GetEnvironment(string sectionName,
        string environmentName)
    {
        var section = GetSection(sectionName);
        var environment = section
            .Environments
            .Single(h => h.EnvironmentId.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
        return new(section, environment);
    }

    private (Section Section, ConfigurationEnvironment Environment, Release Release) GetRelease(string sectionName,
        string environmentName,
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

    private void EnsureTokenSetExistsIfNotNull(string? tokenSetName)
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
        if (section.Environments.Select(h => h.EnvironmentId.Name)
            .Contains(environmentName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The environment already exists: " + environmentName);
        }
    }
}