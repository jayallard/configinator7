using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Model;

internal static class SectionAggregateEventHandlers
{
    internal static void Play(SectionAggregate section, IDomainEvent evt)
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
            case ReleaseValueBecameOld outOfDate:
                OutOfDate(section, outOfDate);
                break;
            case ReleaseValueBecameCurrent current:
                CurrentValue(section, current);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    private static void CreateSection(SectionAggregate section, SectionCreatedEvent evt)
    {
        section.Path = evt.Path;
        
        // todo: id stuff is hacky
        section.Id = evt.SectionId;
        section.SectionName = evt.SectionName;
        if (evt.Schema != null)
        {
            section.InternalSchemas.Add(evt.Schema);
        }
    }

    private static void AddEnvironment(SectionAggregate section, EnvironmentCreatedEvent evt) =>
        section.InternalEnvironments.Add(new EnvironmentEntity(evt.EnvironmentId, evt.EnvironmentName));

    private static void AddSchema(SectionAggregate section, SchemaAddedToSectionEvent evt) =>
        section.InternalSchemas.Add(new SectionSchemaEntity(evt.SectionSchemaId, evt.SchemaVersion, evt.Schema));

    private static void AddRelease(SectionAggregate section, ReleaseCreatedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentId);
        var schema = section.GetSchema(evt.SectionSchemaId);
        var release = new ReleaseEntity(
            evt.ReleaseId,
            schema,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.TokenSetId);
        env.InternalReleases.Add(release);
    }

    private static void AddDeployed(SectionAggregate section, ReleaseDeployedEvent evt)
    {
        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.SetDeployed(true);
        release.InternalDeployments.Add(new DeploymentEntity(
            evt.DeploymentId,
            evt.DeploymentDate));
    }

    private static void RemoveDeployed(SectionAggregate section, DeploymentRemovedEvent evt)
    {
        // todo: need a property for the date
        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.SetDeployed(false);
        release.InternalDeployments.GetDeployment(evt.DeploymentId).RemovedDeployment(evt.EventDate, evt.RemoveReason);
    }

    private static void OutOfDate(SectionAggregate section, ReleaseValueBecameOld evt) =>
        section.GetRelease(evt.EnvironmentId, evt.ReleaseId).SetOutOfDate(true);
    
    private static void CurrentValue(SectionAggregate section, ReleaseValueBecameCurrent evt) =>
        section.GetRelease(evt.EnvironmentId, evt.ReleaseId).SetOutOfDate(false);
}