using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Configinator7.Core.Model;

public class SuperAggregate
{
    #region Temporary

    public Dictionary<string, List<Secret>> TemporarySecretExposure => _secrets;

    #endregion


    // key = SecretModelName.
    private readonly Dictionary<string, List<Secret>> _secrets = new(StringComparer.OrdinalIgnoreCase);


    private void Play(IEvent evt)
    {
        switch (evt)
        {
            case SecretModelCreated(var secretName, var path, var schema):
            {
                var id = new SecretId(secretName, 1);
                var model = new Secret
                {
                    Path = path,
                    Schema = schema,
                    Id = id
                };
                _secrets.Add(secretName, new List<Secret> {model});
                break;
            }
            case HabitatAddedToSecret(var habitatId, var secretId, var value):
            {
                var secret = GetSecret(secretId);
                secret.Habitats.Add(new Habitat
                {
                    HabitatId = habitatId,
                    Value = value
                });
                break;
            }
            case SchemaUpdated(var secretName, var schema):
            {
                // make a copy of the secret and increment the version.
                // add the secret to the history list.
                var secret = GetSecret(secretName);
                var newId = secret.Id with {Version = secret.Id.Version + 1};
                var newSecret = secret with {Id = newId, Schema = schema};
                _secrets[secretName].Add(newSecret);
                break;
            }
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public SecretId CreateSecret(string secretName, string path, JsonSchema schema)
    {
        EnsureSecretDoesntExist(secretName);
        Play(new SecretModelCreated(secretName, path, schema));
        return _secrets[secretName].Last().Id;
    }

    public void UpdateSecretSchema(string secretName, JsonSchema newSchema)
    {
        var secret = GetSecret(secretName);
        var results = new HabitatSchemaValidationResults();
        results.AddRange(secret.Habitats.Select(habitat => new HabitatSchemaValidationResult
            {HabitatId = habitat.HabitatId, ValidationErrors = newSchema.Validate(habitat.Value).ToList()}));

        // todo: will need a FORCE flag. for now, just fail.
        results.EnsureValid();
        Play(new SchemaUpdated(secretName, newSchema));
    }

    public HabitatId AddHabitat(string secretName, string habitatName, JObject secretValue)
    {
        var secret = GetSecret(secretName);
        var value = (JObject) secretValue.DeepClone();

        // resolve value against the schema
        // if it fails, can't assign
        var errors = secret.Schema.Validate(value.ToString());
        if (errors.Any()) throw new InvalidOperationException("Value is invalid. Can't be assigned.");

        var habitatId = new HabitatId(habitatName, 1);
        Play(new HabitatAddedToSecret(habitatId, secret.Id, value));
        return habitatId;
    }

    private Secret GetSecret(string secretName)
    {
        EnsureSecretExists(secretName);
        return _secrets[secretName].Last();
    }

    private Secret GetSecret(SecretId secretId)
    {
        EnsureSecretExists(secretId.Name);
        return _secrets[secretId.Name].Single(s => s.Id == secretId);
    }

    private void EnsureSecretExists(string secretName)
    {
        if (!_secrets.ContainsKey(secretName))
            throw new InvalidOperationException("Secret does not exist: " + secretName);
    }

    private void EnsureSecretDoesntExist(string secretName)
    {
        if (_secrets.ContainsKey(secretName))
            throw new InvalidOperationException("Secret already exists: " + secretName);
    }
}

public interface IEvent
{
};

public record SecretModelCreated(string SecretName, string Path, JsonSchema Schema) : IEvent;

public record HabitatAddedToSecret(HabitatId HabitatId, SecretId SecretId, JObject Value) : IEvent;

public record SchemaUpdated(string SecretName, JsonSchema Schema) : IEvent;