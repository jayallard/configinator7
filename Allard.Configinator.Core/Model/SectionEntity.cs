using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

public class SectionEntity : AggregateBase<SectionId>
{
    internal List<SchemaEntity> InternalSchemas { get; } = new();
    internal List<EnvironmentEntity> InternalEnvironments { get; } = new();
    public IEnumerable<SchemaEntity> Schemas => InternalSchemas.AsReadOnly();
    public IEnumerable<EnvironmentEntity> Environments => InternalEnvironments.AsReadOnly();
    public SectionId Id { get; internal set; }
    public string SectionName { get; internal set; }
    public string Path { get; internal set; }
    public string? TokenSetName { get; internal set; }

    public EnvironmentEntity GetEnvironment(string name) =>
        InternalEnvironments.Single(e => e.EnvironmentName.Equals(name, StringComparison.OrdinalIgnoreCase));

    public EnvironmentEntity GetEnvironment(EnvironmentId environmentId) =>
        InternalEnvironments.GetEnvironment(environmentId);


    internal SectionEntity(SectionId id, string name, string path, SchemaEntity? schema = null,
        string? tokenSetName = null) : base(id)
    {
        Guards.NotDefault(id, nameof(id));
        Guards.HasValue(path, nameof(name));
        Guards.HasValue(path, nameof(path));
        PlaySourceEvent(new SectionCreatedEvent(id, name, path, schema, tokenSetName));
    }

    public SectionEntity(IEnumerable<IDomainEvent> events) : base(new SectionId(3))
    {
        Guards.NotDefault(events, nameof(events));
        foreach (var evt in events) PlaySourceEvent(evt);
        InternalSourceEvents.Clear();
    }

    internal void PlaySourceEvent(IDomainEvent evt)
    {
        SectionAggregateEventHandlers.Play(this, evt);
        InternalSourceEvents.Add(evt);
    }

    public void AddSchema(SchemaEntity schema)
    {
        InternalSchemas.EnsureDoesntExist(schema.Id, schema.Version);
        PlaySourceEvent(new SchemaAddedToSectionEvent(Id, schema));
    }

    public SchemaEntity GetSchema(SchemaId schemaId) =>
        InternalSchemas.Single(s => s.Id == schemaId);

    public EnvironmentEntity AddEnvironment(EnvironmentId environmentId, string name)
    {
        InternalEnvironments.EnsureEnvironmentDoesntExist(environmentId, name);
        PlaySourceEvent(new EnvironmentAddedToSectionEvent(environmentId, Id, name));
        return GetEnvironment(name);
    }
}