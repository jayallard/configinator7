using Allard.Configinator.Core.Model.State;
using Allard.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, SectionOLD> TemporaryExposureSections => _sections;
    public Dictionary<string, TokenSet> TemporaryExposureTokenSets => _tokenSets;
    public List<IEvent> TemporaryExposureEvents => _events;

    #endregion

    // key = Configuration Section Name
    private readonly Dictionary<string, SectionOLD> _sections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TokenSet> _tokenSets = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IEvent> _events = new();

    private void Play(IEvent evt)
    {
        _events.Add(evt);
        switch (evt)
        {
            /*case SectionCreatedEvent(var sectionName, var path, var schema, var tokenSetName):
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
                // add the configuration section to the history list.
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
                    resolved.TokensInUse,
                    resolved.Schema,
                    resolved.EventDate);
                environment.Releases.Add(release);
                break;
            }
            case ReleaseDeployedEvent deployed:
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
            case ReleaseRemovedEvent removed:
            {
                var (_, _, release) =
                    GetRelease(removed.SectionName, removed.EnvironmentName, removed.ReleaseId);
                release.Deployments.Add(new Deployment(removed.EventDate, DeploymentAction.Removed,
                    removed.Reason));
                release.IsDeployed = false;
                break;
            }*/
            case TokenValueSetEvent(var tokenSetName, var key, var value):
            {
                var tokenSet = GetTokenSet(tokenSetName);
                tokenSet.Tokens[key] = value;
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
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public long CreateSection(
        string sectionName,
        ConfigurationSchema? schema,
        string? path,
        string? tokenSetName)
    {
        EnsureSectionDoesntExist(sectionName);
        if (tokenSetName != null)
        {
            EnsureTokenSetExists(tokenSetName);
        }

        Play(new SectionCreatedEvent(new SectionId(0), sectionName, path, schema, tokenSetName));
        return _sections[sectionName].Id;
    }

    public void AddSchema(string sectionName, ConfigurationSchema schema)
    {
        var section = GetSection(sectionName);
        if (section.Schemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        //Play(new SchemaAddedToSection(section.Id, schema));
    }

    public void SetTokenValue(string tokenSetName, string key, JToken value)
    {
        var tokenSet = GetTokenSet(tokenSetName);
        if (tokenSet.Tokens.ContainsKey(key))
        {
            var existing = tokenSet.Tokens[key];
            if (JToken.DeepEquals(value, existing))
            {
                // value didn't change. do nothing.
                return;
            }
        }

        Play(new TokenValueSetEvent(tokenSetName, key, value));

        // find all releases using the token set
        var outOfDate = _sections
            .Values
            .SelectMany(s => s.Environments.SelectMany(
                e => e.Releases.Where(r => string.Equals(r.TokenSet?.TokenSetName, tokenSetName,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(r => new
                    {
                        Release = r,
                        Section = s,
                        Environment = e
                    })))

            // filter down to those using the specific token
            .Where(r => r.Release.UsedTokens.Contains(key));

        // todo: hack. convert to event.
        var resolved = ResolveTokenSet(tokenSetName);
        foreach (var o in outOfDate)
        {
            o.Release.IsOutOfDate = true;

            // compare current resolved tokens with the resolved
            // tokens of the release. if any changes, it's out of date.
            // change to drive off the release Inuse so that only
            // the relevant tokens are considered

            // o.Release.IsOutOfDate = false;
            // if (o.Release.TokenSet == null)
            // {
            //     continue;
            // }
            //
            // if (resolved.Tokens.Count != o.Release.TokenSet.Tokens.Count)
            // {
            //     o.Release.IsOutOfDate = true;
            //     continue;
            // }
            //
            // foreach (var r in resolved.Tokens)
            // {
            //     if (o.Release.TokenSet.Tokens.TryGetValue(r.Key, out var v))
            //     {
            //         if (!JToken.DeepEquals(v.Value, r.Value.Value))
            //         {
            //             o.Release.IsOutOfDate = true;
            //             break;
            //         }
            //     }
            // }
        }

        Console.WriteLine();
    }

    private ICollection<ValidationError> Validate(JObject value, JsonSchema schema) => schema.Validate(value);

    public void AddTokenSet(string name, Dictionary<string, JToken> tokens, string baseTokenSet = null)
    {
        if (baseTokenSet != null)
        {
            EnsureTokenSetExists(baseTokenSet);
        }

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
                .ToDictionary(t => t.Name, t => t.Token, StringComparer.OrdinalIgnoreCase);

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

        TokenSetResolved? ts;
        HashSet<string>? inUse = null;
        if (tokenSetName != null)
        {
            ts = new TokenSetComposer(_tokenSets.Values).Compose(tokenSetName);
            var t2 = ts.Tokens.ToDictionary(k => k.Key, v => v.Value.Token, StringComparer.OrdinalIgnoreCase);
            inUse = JsonUtility.GetTokenNamesDeep(value, t2)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        Play(new ReleaseCreatedEvent(releaseId, sectionName, environmentName, schema, value, resolved, resolvedTokens,
            inUse ?? new HashSet<string>()));
    }

    public TokenSetResolved? ResolveTokenSet(string? tokenSetName) =>
        tokenSetName == null
            ? null
            : new TokenSetComposer(_tokenSets.Values).Compose(tokenSetName);

    private TokenSet GetTokenSet(string tokenSetName)
    {
        EnsureTokenSetExists(tokenSetName);
        return _tokenSets[tokenSetName];
    }

    private (SectionOLD Section, ConfigurationSchema Schemaa) GetSchema(string sectionName, SemanticVersion version)
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
        //Play(new EnvironmentAddedToSectionEvent(environmentName, sectionName));
    }

    public void Deploy(string sectionName, string environmentName, ReleaseId releaseId)
    {
        // var (_, environment, _) = GetRelease(sectionName, environmentName, releaseId);
        //
        // // if a release is already deployed, then set it to removed
        // var released = environment.Releases.SingleOrDefault(r => r.IsDeployed && r.ReleaseId != releaseId);
        // if (released != null)
        // {
        //     Play(new ReleaseRemovedEvent(sectionName, environmentName, released.ReleaseId,
        //         $"Overwritten by Release #{releaseId}"));
        // }
        //
        // // set the new release to deployed
        // Play(new ReleaseDeployedEvent(sectionName, environmentName, releaseId));
    }


    private SectionOLD GetSection(string sectionName)
    {
        EnsureSectionExists(sectionName);
        return _sections[sectionName];
    }

    private (SectionOLD Section, ConfigurationEnvironment Environment) GetEnvironment(string sectionName,
        string environmentName)
    {
        var section = GetSection(sectionName);
        var environment = section
            .Environments
            .Single(h => h.EnvironmentId.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
        return new(section, environment);
    }

    private (SectionOLD Section, ConfigurationEnvironment Environment, Release Release) GetRelease(string sectionName,
        string environmentName,
        long releaseId)
    {
        var (section, environment) = GetEnvironment(sectionName, environmentName);
        var release = environment.Releases.SingleOrDefault(r => r.ReleaseId == releaseId);
        if (release == null)
        {
            throw new InvalidOperationException("Release does not exist");
        }

        return new ValueTuple<SectionOLD, ConfigurationEnvironment, Release>(section, environment, release);
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