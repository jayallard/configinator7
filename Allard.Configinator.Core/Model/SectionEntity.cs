using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public class SectionEntity : EntityBase<SectionEntity, SectionId>, IAggregateRoot
{
    private readonly List<IEvent> _events = new();
    internal List<ConfigurationSchema> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; }= new();
    
    public IEnumerable<IEvent> Events => _events.AsReadOnly();
    public IEnumerable<ConfigurationSchema> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public SectionId Id { get; internal set; }
    public string Name { get; internal set; }
    public string Path { get; internal set; }
    public string? TokenSetName { get; internal set; }
    public EnvironmentEntity GetEnvironment(string name) => throw new NotImplementedException();

    public SectionEntity(SectionId id, string name, string path, ConfigurationSchema? schema = null, string? tokenSetName = null) : base(id)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.HasValue(path, nameof(name));
        Guards.HasValue(path, nameof(path));
        Play(new SectionCreatedEvent(id, name, path, null,  tokenSetName));
    }

    public SectionEntity(IEnumerable<IEvent> events) : base(new SectionId(3))
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) Play(evt);
        _events.Clear();
    }

    private void Play(IEvent evt)
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

        Play(new SchemaAddedToSection(Id, schema));
    }

    public void AddEnvironment(EnvironmentId id, string name)
    {
        if (InternalEnvironments.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Environment already exists. Name=" + name);
        }
        
        Play(new EnvironmentAddedToSectionEvent(id, Id, name));
    }
}