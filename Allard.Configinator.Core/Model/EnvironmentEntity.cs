namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentId>
{
    internal EnvironmentEntity(
        EnvironmentId id,
        string environmentType,
        string environmentName)
    {
        Id = id;
        EnviromentType = environmentType;
        EnvironmentName = environmentName;
    }

    internal List<ReleaseEntity> InternalReleases { get; } = new();
    public IEnumerable<ReleaseEntity> Releases => InternalReleases.AsReadOnly();
    public string EnviromentType { get; }
    public string EnvironmentName { get; }
}