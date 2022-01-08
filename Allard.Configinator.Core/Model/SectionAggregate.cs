using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

public class SectionAggregate
{
    private Section Section { get; } = new();

    private readonly List<IEvent> _events = new();

    public string Name => Section.Name;

    public SectionAggregate(long id, string name, string path)
    {
        Play(new SectionCreatedEvent(id, name, path, null, null));    
    }

    public long SectionId => Section.Id;
    
    public SectionAggregate(IEnumerable<IEvent> events)
    {
        foreach (var evt in events) Play(evt);
        _events.Clear();
    }

    private void Play(IEvent evt)
    {
        SectionAggregateEventHandlers.Play(Section, evt);
        _events.Add(evt);
    }

    public void AddSchema(ConfigurationSchema schema)
    {
        if (Section.Schemas.Any(s => s.Version == schema.Version))
        {
            throw new InvalidOperationException("Schema already exists. Version=" + schema.Version);
        }

        Play(new SchemaAddedToSection(Section.Id, schema));
    }
}

internal static class SectionAggregateEventHandlers
{
    public static void Play(Section section, IEvent evt)
    {
        switch (evt)
        {
            case SectionCreatedEvent create:
                CreateSection(section, create);
                break;
            case EnvironmentAddedToSectionEvent environmentAdded:
                AddEnvironment(section, environmentAdded);
                break;
            case SchemaAddedToSection schemaAdded:
                AddSchema(section, schemaAdded);
                break;
            case ReleaseCreatedEvent releaseCreated:
                AddRelease(section, releaseCreated);
                break;
            case ReleaseDeployedEvent deployed:
                AddDeployed(section, deployed);
                break;
            case ReleaseRemovedEvent removed:
                RemoveDeployed(section, removed);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    public static void CreateSection(Section section, SectionCreatedEvent evt)
    {
        section.Path = evt.Path;
        section.Id = 0;
        section.Name = evt.SectionName;
        section.TokenSetName = evt.TokenSetName;
        if (evt.Schema != null)
        {
            section.Schemas.Add(evt.Schema);
        }
    }

    public static void AddEnvironment(Section section, EnvironmentAddedToSectionEvent evt)
    {
        section.Environments.Add(new ConfigurationEnvironment
        {
            EnvironmentId = new ConfigurationEnvironmentId(evt.EnvironmentName),
        });
    }

    public static void AddSchema(Section section, SchemaAddedToSection evt)
    {
        section.Schemas.Add(evt.Schema);
    }

    public static void AddRelease(Section section, ReleaseCreatedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentName);
        var release = new Release(
            evt.ReleaseId,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.Tokens,
            evt.TokensInUse,
            evt.Schema,
            evt.EventDate);
        env.Releases.Add(release);
    }

    private static void AddDeployed(Section section, ReleaseDeployedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentName);
        var release = env.GetRelease(evt.ReleaseId);

        // set all of the deployments to NOT DEPLOYED
        foreach (var d in env.Releases.SelectMany(r => r.Deployments))
        {
            d.IsDeployed = false;
        }

        release.Deployments.Add(new Deployment(evt.EventDate, DeploymentAction.Deployed, string.Empty)
            {IsDeployed = true});
        release.IsDeployed = true;
    }

    private static void RemoveDeployed(Section section, ReleaseRemovedEvent evt)
    {
        var release = section.GetEnvironment(evt.EnvironmentName).GetRelease(evt.ReleaseId);
        release.Deployments.Add(new Deployment(evt.EventDate, DeploymentAction.Removed, evt.Reason));
        release.IsDeployed = false;
    }
}