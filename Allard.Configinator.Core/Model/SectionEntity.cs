using Allard.Configinator.Core.Model.State;
using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SectionEntity : EntityBase<SectionId>, IAggregateRoot
{
    private readonly List<ISourceEvent> _events = new();
    internal List<ConfigurationSchema> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; }= new();
    public IEnumerable<ISourceEvent> Events => _events.AsReadOnly();
    public IEnumerable<ConfigurationSchema> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public SectionId Id { get; internal set; }
    public string SectionName { get; internal set; }
    public string Path { get; internal set; }
    public string? TokenSetName { get; internal set; }

    public EnvironmentEntity GetEnvironment(string name) =>
        InternalEnvironments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));

    public SectionEntity(SectionId id, string name, string path, ConfigurationSchema? schema = null, string? tokenSetName = null) : base(id)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.HasValue(path, nameof(name));
        Guards.HasValue(path, nameof(path));
        PlaySourceEvent(new SectionCreatedSourceEvent(id, name, path, schema,  tokenSetName));
    }

    public SectionEntity(IEnumerable<ISourceEvent> events) : base(new SectionId(3))
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) PlaySourceEvent(evt);
        _events.Clear();
    }

    internal void PlaySourceEvent(ISourceEvent evt)
    {
        SectionAggregateEventHandlers.Play(this, evt);
        _events.Add(evt);
    }

    public void AddSchema(ConfigurationSchema schema)
    {
        if (InternalSchemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        PlaySourceEvent(new SchemaAddedToSection(Id, schema));
    }

    public ConfigurationSchema GetSchema(SemanticVersion schemaVersion) =>
        InternalSchemas.Single(s => s.Version == schemaVersion);

    public EnvironmentEntity AddEnvironment(EnvironmentId environmentId, string name)
    {
        if (InternalEnvironments.Any(s => s.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Environment already exists. Name=" + name);
        }
        
        if (InternalEnvironments.Any(s => s.Id == environmentId))
        {
            throw new InvalidOperationException("Environment already exists. Id=" + environmentId.Id);
        }
        
        PlaySourceEvent(new EnvironmentAddedToSectionSourceEvent(environmentId, Id, name));
        return GetEnvironment(name);
    }
}