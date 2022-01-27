using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

internal static class SectionAggregateEventHandlers
{
    public static void Play(SectionEntity section, IDomainEvent evt)
    {
        switch (evt)
        {
            case SectionCreatedEvent create:
                CreateSection(section, create);
                break;
            case EnvironmentCreatedEvent environmentAdded:
                AddEnvironment(section, environmentAdded);
                break;
            case SchemaAddedToSectionEvent schemaAdded:
                AddSchema(section, schemaAdded);
                break;
            case ReleaseCreatedEvent releaseCreated:
                AddRelease(section, releaseCreated);
                break;
            case ReleaseDeployedEvent deployed:
                AddDeployed(section, deployed);
                break;
            case DeploymentRemovedEvent removed:
                RemoveDeployed(section, removed);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    public static void CreateSection(SectionEntity section, SectionCreatedEvent evt)
    {
        section.Path = evt.Path;
        section.Id = evt.SectionId;
        section.SectionName = evt.SectionName;
        section.TokenSetName = evt.TokenSetName;
        if (evt.Schema != null)
        {
            section.InternalSchemas.Add(evt.Schema);
        }
    }

    public static void AddEnvironment(SectionEntity section, EnvironmentCreatedEvent evt) =>
        section.InternalEnvironments.Add(new EnvironmentEntity(evt.EnvironmentId, evt.EnvironmentName));

    public static void AddSchema(SectionEntity section, SchemaAddedToSectionEvent evt) =>
        section.InternalSchemas.Add(new SchemaEntity(evt.SchemaId, evt.SchemaVersion, evt.Schema));

    public static void AddRelease(SectionEntity section, ReleaseCreatedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentId);
        var schema = section.GetSchema(evt.SchemaId);
        var release = new ReleaseEntity(
            evt.ReleaseId,
            schema,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.Tokens);
        env.InternalReleases.Add(release);
    }

    private static void AddDeployed(SectionEntity section, ReleaseDeployedEvent evt)
    {
        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.InternalDeployments.Add(new DeploymentHistoryEntity(
            evt.DeploymentHistoryId, 
            evt.DeploymentDate, 
            DeploymentHistoryType.Deployed, 
            null));
    }

    private static void RemoveDeployed(SectionEntity section, DeploymentRemovedEvent evt)
    {
        var deployment = section.GetDeployment(evt.EnvironmentId, evt.ReleaseId, evt.DeploymentHistoryId);
        deployment.SetRemoved();
    }
}