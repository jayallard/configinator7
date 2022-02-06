﻿namespace Allard.Configinator.Core.Model;

public class EnvironmentEntity : EntityBase<EnvironmentId>
{
    internal List<ReleaseEntity> InternalReleases { get; } = new();
    public IEnumerable<ReleaseEntity> Releases => InternalReleases.AsReadOnly();
    public string EnvironmentName { get; }
    public EnvironmentEntity(EnvironmentId id, string environmentName)
    {
        Id = id;
        EnvironmentName = environmentName;
    }
}