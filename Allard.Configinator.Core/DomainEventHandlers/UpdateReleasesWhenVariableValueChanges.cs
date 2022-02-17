using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
/// When a variable value is updated, then find all releases that use the variable.
/// Resolve the release's model value against the current versions of the variable sets.
/// If the resolved value is different than the release's resolved value, then the
/// release is out of date.
/// For example: When the release is created, the value of the PASSWORD variable is ABC.
/// The password variable value is changed to XYZ.
/// The existing release is now out of date because the password has changed.
/// </summary>
public class UpdateReleasesWhenVariableValueChanges : IEventHandler<VariableValueSetEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VariableSetDomainService _variableSetDomainService;

    public UpdateReleasesWhenVariableValueChanges(IUnitOfWork unitOfWork, VariableSetDomainService variableSetDomainService)
    {
        _variableSetDomainService = Guards.HasValue(variableSetDomainService, nameof(variableSetDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(VariableValueSetEvent evt, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Variable changed: " + evt);

        // get the composer for all variable sets
        var variableSet = await _variableSetDomainService.GetVariableSetComposedAsync(evt.VariableSetName, cancellationToken);
        var values = variableSet.ToValueDictionary();
        var related = variableSet.GetRelatedVariableSetNames();

        // get all releases that use the variable set and variable
        var releases =
            await GetReleasesThatUseToken(evt.VariableSetName, evt.VariableName, related, cancellationToken);
        foreach (var release in releases)
        {
            // use the release's ModelValue, and the current VariableSet values.
            // Resolve, and see if the new value is different than the release's value.
            // if so, then the release is out of date.
            var resolved = await JsonUtility.ResolveAsync(release.Release.ModelValue.ToJsonNetJson(), values,
                cancellationToken);
            var changed = !JToken.DeepEquals(resolved, release.Release.ResolvedValue.ToJsonNetJson());
            release.Section.SetOutOfDate(release.Environment.Id, release.Release.Id, changed);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IEnumerable<SectionAggregate>> GetAffectedSections(
        string variableSetName,
        string variableName,
        CancellationToken cancellationToken) =>
        (await _unitOfWork.Sections.FindAsync(new UsesVariable(variableSetName, variableName), cancellationToken))
        .ToList();

    /// <summary>
    /// Returns all releases that use the variable set and variable.
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="relatedVariableSets"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="variableSetName"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ReleaseDetails>> GetReleasesThatUseToken(
        string variableSetName,
        string variableName,
        ISet<string> relatedVariableSets,
        CancellationToken cancellationToken)
    {
        return (await GetAffectedSections(variableSetName, variableName, cancellationToken))
            .SelectMany(s =>
                // sections have environments, environments have releases, releases use variable sets
                // get all the releases that use the changed variable of the changed variable set
                s.InternalEnvironments.SelectMany(
                    e => e.InternalReleases
                        .Where(r =>
                            // has a variable set
                            r.VariableSetId != null

                            // the variable set is in the hierarchy of the variable set that changed.
                            // IE: if there's a hierarchy of variable sets, then the change
                            // can affect any release that's using any descendant of the changed
                            // variable set.
                            // TODO fix
                            // && relatedVariableSets.Contains(r.VariableSet.VariableSetName)

                            // the release is using the variable that changed
                            && r.VariablesInUse.Contains(variableName))
                        .Select(r => new ReleaseDetails(s, e, r))
                ));
    }

    private record ReleaseDetails(SectionAggregate Section, EnvironmentEntity Environment, ReleaseEntity Release);
}