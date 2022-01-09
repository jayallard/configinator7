namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentEntity, EnvironmentId>
{
    public EnvironmentEntity(EnvironmentId id, string name) : base(id)
    {
        Name = name;
    }
    
    public string Name { get; }
    
    internal readonly List<ReleaseEntity> _releases = new();

    public IEnumerable<ReleaseEntity> Releases => _releases.AsReadOnly();

    public ReleaseEntity GetRelease(ReleaseId id) => throw new NotImplementedException();
}