using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using NuGet.Versioning;

namespace Configinator7.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, Secret> TemporarySecretExposure => _secrets;

    #endregion

    // key = SecretModelName.
    private readonly Dictionary<string, Secret> _secrets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, JToken>> _tokenSets = new(StringComparer.OrdinalIgnoreCase);

    private void Play(IEvent evt)
    {
        switch (evt)
        {
            case SecretCreatedEvent(var secretName, var path, var schema, var tokenSetName):
            {
                var id = new SecretId(secretName);
                var secret = new Secret
                {
                    Path = path,
                    Id = id,
                    TokenSetName = tokenSetName
                };
                if (schema != null)
                {
                    secret.Schemas.Add(schema);
                }
                
                _secrets.Add(secretName, secret);
                break;
            }
            case HabitatAddedToSecretEvent(var habitatName, var secretName):
            {
                var secret = GetSecret(secretName);
                secret.Habitats.Add(new Habitat
                {
                    HabitatId = new HabitatId(habitatName),
                });
                break;
            }
            case SchemaAddedToSecret(var secretName, var schema):
            {
                // make a copy of the secret and increment the version.
                // add the secret to the history list.
                GetSecret(secretName).Schemas.Add(schema);
                break;
            }
            case ReleaseCreatedEvent resolved:
            {
                var (_, habitat, schema) = GetSchema(resolved.SecretName, resolved.HabitatName, resolved.Version);
                var release = new Release(
                    resolved.ModelValue,
                    resolved.ResolvedValue, 
                    resolved.Tokens,
                    resolved.Version, 
                    null);
                habitat.Releases.Add(release);
                break;
            }
            case SchemaAddedToHabitatEvent(var secretName, var habitatName, var schema):
            {
                var stuff = GetHabitat(secretName, habitatName);
                stuff.Habitat.Schemas.Add(schema);
                break;
            }
            case TokenSetCreatedEvent created:
            {
                _tokenSets[created.TokenSetName] = created.Tokens;
                break;
            }
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public SecretId CreateSecret(string secretName,
        ConfigurationSchema? schema,
        string? path,
        string? tokenSetName)
    {
        EnsureSecretDoesntExist(secretName);
        EnsureTokenSetExists(tokenSetName);
        Play(new SecretCreatedEvent(secretName, path, schema, tokenSetName));
        return _secrets[secretName].Id;
    }

    public void AddSchema(string secretName, ConfigurationSchema schema)
    {
        var secret = GetSecret(secretName);
        if (secret.Schemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        Play(new SchemaAddedToSecret(secretName, schema));
    }

    private ICollection<ValidationError> Validate(JObject value, JsonSchema schema) => schema.Validate(value);

    public void AddSchemaToHabitat(
        string secretName,
        string habitatName,
        SemanticVersion version)
    {
        var secret = GetSecret(secretName);
        var habitat = GetHabitat(secretName, habitatName);
        var schema = secret.Schemas.SingleOrDefault(s => s.Version == version);
        if (schema == null)
        {
            throw new InvalidOperationException("Schema doesn't exist.");
        }

        if (habitat.Habitat.Schemas.Any(s => s.Version == version))
        {
            throw new InvalidOperationException("The schema is already assigned to the habitat.");
        }

        Play(new SchemaAddedToHabitatEvent(secretName, habitatName, schema));
    }

    public void CreateTokenSet(string name, Dictionary<string, JToken> tokens)
    {
        EnsureTokenSetDoesntExist(name);
        tokens = tokens.ToDictionary(t => t.Key, t => t.Value?.DeepClone());
        Play(new TokenSetCreatedEvent(name, tokens));
    }
    
    public async Task CreateReleaseAsync(
        string secretName,
        string habitatName,
        SemanticVersion schemaVersion,
        JObject value)
    {
        var (_, _, schema) = GetSchema(secretName, habitatName, schemaVersion);
        var resolved = await JsonUtility.ResolveAsync(value, new Dictionary<string, JToken>());
        var results = Validate(resolved, schema.Schema);
        if (results.Any())
        {
            throw new InvalidOperationException("schema validation failed");
        }

        Play(new ReleaseCreatedEvent(secretName, habitatName, schemaVersion, value, resolved, null));
    }

    private (Secret Secret, Habitat Habitat, ConfigurationSchema Schemaa)  GetSchema(string secretName, string habitatName, SemanticVersion version)
    {
        var habitat = GetHabitat(secretName, habitatName);
        var schema = habitat.Habitat.Schemas.SingleOrDefault(s => s.Version == version);
        if (schema == null) throw new InvalidOperationException($"Schema doesnt' exist. Secret={secretName}, Version={version.ToFullString()}");
        return new(habitat.Secret, habitat.Habitat, schema);
    }

    public void AddHabitat(string secretName, string habitatName)
    {
        EnsureHabitatDoesntExist(secretName, habitatName);
        var secret = GetSecret(secretName);
        //var value = (JObject) secretValue.DeepClone();

        // resolve value against the schema
        // if it fails, can't assign
        // var errors = secret.sc.Validate(value.ToString());
        // if (errors.Any()) throw new InvalidOperationException("Value is invalid. Can't be assigned.");
        Play(new HabitatAddedToSecretEvent(habitatName, secretName));
    }

    private Secret GetSecret(string secretName)
    {
        EnsureSecretExists(secretName);
        return _secrets[secretName];
    }

    private (Secret Secret, Habitat Habitat) GetHabitat(string secretName, string habitatName)
    {
        var secret = GetSecret(secretName);
        var habitat = secret
            .Habitats
            .Single(h => h.HabitatId.Name.Equals(habitatName, StringComparison.OrdinalIgnoreCase));
        return new(secret, habitat);
    }

    private void EnsureSecretExists(string secretName)
    {
        if (!_secrets.ContainsKey(secretName))
            throw new InvalidOperationException("Secret does not exist: " + secretName);
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
    
    private void EnsureSecretDoesntExist(string secretName)
    {
        if (_secrets.ContainsKey(secretName))
            throw new InvalidOperationException("Secret already exists: " + secretName);
    }

    private void EnsureHabitatDoesntExist(string secretName, string habitatName)
    {
        var secret = GetSecret(secretName);
        if (secret.Habitats.Select(h => h.HabitatId.Name).Contains(habitatName, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The habitat already exists: " + habitatName);
        }
    }
}
    