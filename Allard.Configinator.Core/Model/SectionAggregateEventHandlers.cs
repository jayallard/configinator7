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
        section.Id = evt.SectionId;
        section.SectionName = evt.SectionName;
        section.Namespace = evt.Namespace;
        section.InternalEnvironmentTypes.Add(evt.InitialEnvironmentType);
    }

    private static void AddEnvironment(
        SectionAggregate section,
        EnvironmentCreatedEvent evt)
    {
        section.InternalEnvironments.Add(new EnvironmentEntity(
            evt.EnvironmentId,
            evt.EnvironmentType,
            evt.EnvironmentName));
    }

    private static void AddSchema(SectionAggregate section, SchemaAddedToSectionEvent evt)
    {
        section.InternalSchemas.Add(evt.SchemaId);
    }

    private static void AddRelease(SectionAggregate section, ReleaseCreatedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentId);
        var release = new ReleaseEntity(
            evt.ReleaseId,
            evt.SchemaId,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.VariableSetId);
        env.InternalReleases.Add(release);
    }

    private static void AddDeployed(SectionAggregate section, ReleaseDeployedEvent evt)
    {
        // if an active deployment exists, remove it
        section.SetActiveDeploymentToRemoved(evt.EnvironmentId, evt.DeploymentId);

        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.SetDeployed(true);
        release.InternalDeployments.Add(new DeploymentEntity(
            evt.DeploymentId,
            evt.DeploymentDate,
            evt.DeploymentResult,
            evt.Notes));
    }

    private static void RemoveDeployed(SectionAggregate section, DeploymentRemovedEvent evt)
    {
        // todo: need a property for the date
        var release = section.GetRelease(evt.EnvironmentId, evt.ReleaseId);
        release.SetDeployed(false);
        release.InternalDeployments.GetDeployment(evt.DeploymentId).RemovedDeployment(evt.EventDate, evt.RemoveReason);
    }

    private static void OutOfDate(SectionAggregate section, ReleaseValueBecameOld evt)
    {
        section.GetRelease(evt.EnvironmentId, evt.ReleaseId).SetOutOfDate(true);
    }

    private static void CurrentValue(SectionAggregate section, ReleaseValueBecameCurrent evt)
    {
        section.GetRelease(evt.EnvironmentId, evt.ReleaseId).SetOutOfDate(false);
    }
}