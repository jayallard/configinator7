namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentId>
{
    public EnvironmentEntity(
        EnvironmentId id,
        string environmentType,
        string environmentName)
    {
        Id = id;
        EnvironmentType = environmentType;
        EnvironmentName = environmentName;
    }

    internal List<ReleaseEntity> InternalReleases { get; } = new();
    public IEnumerable<ReleaseEntity> Releases => InternalReleases.AsReadOnly();
    public string EnvironmentType { get; }
    public string EnvironmentName { get; }
}