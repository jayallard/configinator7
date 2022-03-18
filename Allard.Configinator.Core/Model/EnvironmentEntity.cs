using System.Text.Json.Serialization;

namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentId>
{
    protected internal readonly List<ReleaseEntity> _releases = new();

    public EnvironmentEntity(
        EnvironmentId id,
        string environmentType,
        string environmentName)
    {
        Id = id;
        EnvironmentType = environmentType;
        EnvironmentName = environmentName;
    }

    [JsonInclude]
    public IEnumerable<ReleaseEntity> Releases
    {
        get => _releases.AsReadOnly();
        private init => _releases = value.ToList();
    }

    public string EnvironmentType { get; private init; }
    public string EnvironmentName { get; private init; }
}