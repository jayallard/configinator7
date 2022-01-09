namespace Allard.Configinator.Core.Model;

internal static class SectionAggregateEventHandlers
{
    public static void Play(SectionEntity section, IEvent evt)
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
            // case ReleaseCreatedEvent releaseCreated:
            //     AddRelease(section, releaseCreated);
            //     break;
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

    public static void CreateSection(SectionEntity section, SectionCreatedEvent evt)
    {
        section.Path = evt.Path;
        section.Id = evt.SectionId;
        section.Name = evt.SectionName;
        section.TokenSetName = evt.TokenSetName;
        if (evt.Schema != null)
        {
            section.InternalSchemas.Add(evt.Schema);
        }
    }

    public static void AddEnvironment(SectionEntity section, EnvironmentAddedToSectionEvent evt)
    {
        section.InternalEnvironments.Add(new EnvironmentEntity(evt.EnvironmentId, evt.Name));
    }

    public static void AddSchema(SectionEntity section, SchemaAddedToSection evt)
    {
        section.InternalSchemas.Add(evt.Schema);
    }

    // public static void AddRelease(SectionAggregate section, ReleaseCreatedEvent evt)
    // {
    //     var env = section.GetEnvironment(evt.EnvironmentName);
    //     var release = new ReleaseEntity();
    //     // (
    //     //     evt.ReleaseId,
    //     //     evt.ModelValue,
    //     //     evt.ResolvedValue,
    //     //     evt.Tokens,
    //     //     evt.TokensInUse,
    //     //     evt.Schema,
    //     //     evt.EventDate);
    //     env._releases.Add(release);
    // }

    private static void AddDeployed(SectionEntity section, ReleaseDeployedEvent evt)
    {
        var env = section.GetEnvironment(evt.EnvironmentName);
        var release = env.GetRelease(evt.ReleaseId);

        // set all of the deployments to NOT DEPLOYED
        // foreach (var d in env.Releases.SelectMany(r => r.Deployments))
        // {
        //     d.IsDeployed = false;
        // }
        //
        // release.Deployments.Add(new Deployment(evt.EventDate, DeploymentAction.Deployed, string.Empty)
        //     {IsDeployed = true});
        // release.IsDeployed = true;
    }

    private static void RemoveDeployed(SectionEntity section, ReleaseRemovedEvent evt)
    {
        var release = section.GetEnvironment(evt.EnvironmentName).GetRelease(evt.ReleaseId);
        // release.Deployments.Add(new Deployment(evt.EventDate, DeploymentAction.Removed, evt.Reason));
        // release.IsDeployed = false;
    }
}