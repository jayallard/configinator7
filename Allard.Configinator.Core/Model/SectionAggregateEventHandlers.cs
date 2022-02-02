using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

internal static class SectionAggregateEventHandlers
{
    internal static void Play(SectionEntity section, IDomainEvent evt)
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
            case ReleaseFellOutOfDate outOfDate:
                OutOfDate(section, outOfDate);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    private static void CreateSection(SectionEntity section, SectionCreatedEvent evt)
    {
        section.Path = evt.Path;
        section.Id = evt.SectionId;
        section.SectionName = evt.SectionName;
        if (evt.Schema != null)
        {
            section.InternalSchemas.Add(evt.Schema);
        }
    }

    private static void AddEnvironment(SectionEntity section, EnvironmentCreatedEvent evt) =>
        section.InternalEnvironments.Add(new EnvironmentEntity(evt.EnvironmentId, evt.EnvironmentName));

    private static void AddSchema(SectionEntity section, SchemaAddedToSectionEvent evt) =>
        section.InternalSchemas.Add(new SchemaEntity(evt.SchemaId, evt.SchemaVersion, evt.Schema));

    private static void AddRelease(SectionEntity section, ReleaseCreatedEvent evt)
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
        release.SetDeployed(true);
        release.InternalDeployments.Add(new DeploymentEntity(
            evt.DeploymentId,
            evt.DeploymentDate));
    }

    private static void RemoveDeployed(SectionEntity section, DeploymentRemovedEvent evt)
    {
        // todo: need a property for the date
        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.SetDeployed(false);
        release.InternalDeployments.GetDeployment(evt.DeploymentId).RemovedDeployment(evt.EventDate, evt.RemoveReason);
    }

    private static void OutOfDate(SectionEntity section, ReleaseFellOutOfDate evt) =>
        section.GetRelease(evt.EnvironmentId, evt.ReleaseId).SetOutOfDate(true);
}