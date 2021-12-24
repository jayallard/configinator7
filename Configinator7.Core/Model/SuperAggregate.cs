using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
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


    private void Play(IEvent evt)
    {
        switch (evt)
        {
            case SecretCreated(var secretName, var path, var schema):
            {
                var id = new SecretId(secretName);
                var secret = new Secret
                {
                    Path = path,
                    Schemas = {schema},
                    Id = id
                };
                _secrets.Add(secretName, secret);
                break;
            }
            case HabitatAddedToSecret(var habitatName, var secretName):
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
            case ValueResolved resolved:
            {
                var (secret, habitat, schema) = GetSchema(resolved.SecretName, resolved.HabitatName, resolved.Version);
                var v = new ResolvedConfigurationValue(resolved.ModelValue, resolved.ResolvedValue, resolved.Tokens,
                    null);
                schema.Resolved.Add(v);
                break;
            }
            case SchemaAddedToHabitat(var secretName, var habitatName, var schema):
            {
                var stuff = GetHabitat(secretName, habitatName);
                var s = new HabitatSchema
                {
                    ModelValue = null,
                    Schema = schema
                };
                stuff.Habitat.Schemas.Add(s);
                break;
            }
            case ValueSet valueSet:
            {
                var (secret, habitat, schema) =
                    GetSchema(valueSet.SecretName, valueSet.HabitatName, valueSet.SchemaVersion);
                schema.ModelValue = valueSet.Value;
                break;
            }
            default:
                throw new NotImplementedException(evt.GetType().FullName);
        }
    }

    public SecretId CreateSecret(string secretName, string path, ConfigurationSchema schema)
    {
        EnsureSecretDoesntExist(secretName);
        Play(new SecretCreated(secretName, path, schema));
        return _secrets[secretName].Id;
    }

    private JObject Resolve(JObject value)
    {
        return value;
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

        if (habitat.Habitat.Schemas.Any(s => s.Schema.Version == version))
        {
            throw new InvalidOperationException("The schema is already assigned to the habitat.");
        }

        Play(new SchemaAddedToHabitat(secretName, habitatName, schema));
    }

    public void SetValue(
        string secretName,
        string habitatName,
        SemanticVersion schemaVersion,
        JObject value)
    {
        var habitat = GetHabitat(secretName, habitatName);
        var _ = habitat.Habitat.Schemas.Single(s => s.Schema.Version == schemaVersion);
        Play(new ValueSet(secretName, habitatName, schemaVersion, value));
    }

    // public void UpdateSecretSchema(string secretName, ConfigurationSchema newSchema)
    // {
    //     var secret = GetSecret(secretName);
    //
    //     var results = secret
    //         .Habitats
    //         .Select(habitat =>
    //         {
    //             var resolved = Resolve(habitat.ModelValue);
    //             var validationResults = Validate(resolved, newSchema);
    //             var result = new HabitatSchemaValidationResult
    //             {
    //                 Schema = newSchema,
    //                 HabitatId = habitat.HabitatId,
    //                 ValidationErrors = validationResults,
    //                 ModelValue = habitat.ModelValue
    //             };
    //
    //             return new
    //             {
    //                 Habitat = habitat,
    //                 Result = result
    //             };
    //         })
    //         .ToArray();
    //
    //     var resultSet = new HabitatSchemaValidationResults();
    //     resultSet.AddRange(results.Select(r => r.Result));
    //     resultSet.EnsureValid();
    //
    //     Play(new SchemaAdded(secretName, newSchema));
    //     foreach (var r in results)
    //     {
    //         if (JToken.DeepEquals(r.Result.Value, r.Habitat.ResolvedValue))
    //         {
    //             // resolved value didn't change
    //             continue;
    //         }
    //
    //         Play(new ValueResolved(secretName, r.Habitat.HabitatId.Name, r.Result.Value));
    //     }
    // }

    public void Resolve(
        string secretName,
        string habitatName,
        SemanticVersion schemaVersion)
    {
        var (_, _, schema) = GetSchema(secretName, habitatName, schemaVersion);
        var resolved = Resolve(schema.ModelValue);
        var results = Validate(resolved, schema.Schema.Schema);
        if (results.Any())
        {
            throw new InvalidOperationException("schema validation failed");
        }

        Play(new ValueResolved(secretName, habitatName, schemaVersion, schema.ModelValue, resolved, null));
    }

    private (Secret Secret, Habitat Habitat, HabitatSchema HabitatSchema)  GetSchema(string secretName, string habitatName, SemanticVersion version)
    {
        var habitat = GetHabitat(secretName, habitatName);
        var schema = habitat.Habitat.Schemas.Single(s => s.Schema.Version == version);
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
        Play(new HabitatAddedToSecret(habitatName, secretName));
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

public interface IEvent
{
};

public record SecretCreated(string SecretName, string Path, ConfigurationSchema Schema) : IEvent;

public record HabitatAddedToSecret(string HabitatName, string SecretName) : IEvent;

public record SchemaAddedToSecret(string SecretName, ConfigurationSchema Schema) : IEvent;

public record SchemaAddedToHabitat(
    string SecretName,
    string HabitatName,
    ConfigurationSchema Schema) : IEvent;

public record ValueSet(
    string SecretName,
    string HabitatName,
    SemanticVersion SchemaVersion,
    JObject Value) : IEvent;

public record ValueResolved(
    string SecretName,
    string HabitatName,
    SemanticVersion Version,
    JObject ModelValue,
    JObject ResolvedValue,
    TokenSet Tokens) : IEvent;