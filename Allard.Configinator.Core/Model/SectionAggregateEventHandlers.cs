using Allard.Configinator.Core.Model.State;

namespace Allard.Configinator.Core.Model;

internal static class SectionAggregateEventHandlers
{
    public static void Play(SectionEntity section, ISourceEvent evt)
    {
        switch (evt)
        {
            case SectionCreatedSourceEvent create:
                CreateSection(section, create);
                break;
            case EnvironmentAddedToSectionSourceEvent environmentAdded:
                AddEnvironment(section, environmentAdded);
                break;
            case SchemaAddedToSection schemaAdded:
                AddSchema(section, schemaAdded);
                break;
            case ReleaseCreatedSourceEvent releaseCreated:
                AddRelease(section, releaseCreated);
                break;
            case ReleaseDeployedSourceEvent deployed:
                AddDeployed(section, deployed);
                break;
            case ReleaseRemovedSourceEvent removed:
                RemoveDeployed(section, removed);
                break;
            default:
                throw new NotImplementedException("Unhandled event: " + evt.GetType().FullName);
        }
    }

    public static void CreateSection(SectionEntity section, SectionCreatedSourceEvent evt)
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

    public static void AddEnvironment(SectionEntity section, EnvironmentAddedToSectionSourceEvent evt) =>
        section.InternalEnvironments.Add(new EnvironmentEntity(section, evt.EnvironmentId, evt.Name));

    public static void AddSchema(SectionEntity section, SchemaAddedToSection evt) =>
        section.InternalSchemas.Add(evt.Schema);

    public static void AddRelease(SectionEntity section, ReleaseCreatedSourceEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentName);
        var release = new ReleaseEntity(
            evt.ReleaseId,
            env,
            evt.Schema,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.Tokens);
        env.InternalReleases.Add(release);
    }

    private static void AddDeployed(SectionEntity section, ReleaseDeployedSourceEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentName);
        var release = env.GetRelease(evt.ReleaseId);
        release.InternalDeployments.Add(new DeploymentEntity(
            evt.DeploymentId, 
            release, 
            evt.deploymentDate, 
            DeploymentAction.Deployed, 
            null));
    }

    private static void RemoveDeployed(SectionEntity section, ReleaseRemovedSourceEvent evt)
    {
        var release = section.GetEnvironment(evt.EnvironmentName).GetRelease(evt.ReleaseId);
        // release.Deployments.Add(new Deployment(evt.EventDate, DeploymentAction.Removed, evt.Reason));
        // release.IsDeployed = false;
    }
}