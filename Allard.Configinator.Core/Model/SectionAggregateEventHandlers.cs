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
            case DeploymentRemovedSourceEvent removed:
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
        section.InternalEnvironments.Add(new EnvironmentEntity(section, evt.EnvironmentId, evt.EnvironmentName));

    public static void AddSchema(SectionEntity section, SchemaAddedToSection evt) =>
        section.InternalSchemas.Add(evt.Schema);

    public static void AddRelease(SectionEntity section, ReleaseCreatedSourceEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentId);
        var schema = section.GetSchema(evt.SchemaId);
        var release = new ReleaseEntity(
            evt.ReleaseId,
            env,
            schema,
            evt.ModelValue,
            evt.ResolvedValue,
            evt.Tokens);
        env.InternalReleases.Add(release);
    }

    private static void AddDeployed(SectionEntity section, ReleaseDeployedSourceEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentId);
        var release = env.GetRelease(evt.ReleaseId);
        release.InternalDeployments.Add(new DeploymentHistoryEntity(
            evt.DeploymentId, 
            release, 
            evt.deploymentDate, 
            DeploymentAction.Deployed, 
            null));
    }

    private static void RemoveDeployed(SectionEntity section, DeploymentRemovedSourceEvent evt)
    {
        var deployment = section
            .GetEnvironment(evt.EnvironmentId)
            .GetRelease(evt.ReleaseId)
            .GetDeployment(evt.DeploymentId);
        deployment.SetRemoved();
    }
}