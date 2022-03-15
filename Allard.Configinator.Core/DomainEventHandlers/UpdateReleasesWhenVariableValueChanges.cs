using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;
using Allard.Json;
using Newtonsoft.Json.Linq;

namespace Allard.Configinator.Core.DomainEventHandlers;

/// <summary>
///     Determine which releases are out of date when a variable value changes.
///     When a variable value is updated, then find all releases that use the variable.
///     Resolve the release's model value against the current versions of the variable sets.
///     If the resolved value is different than the release's resolved value, then the
///     release is out of date.
///     For example: When the release is created, the value of the PASSWORD variable is ABC.
///     The password variable value is changed to XYZ.
///     The existing release is now out of date because the password has changed.
/// </summary>
public class UpdateReleasesWhenVariableValueChanges : IDomainEventHandler<VariableValueSetEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly VariableSetDomainService _variableSetDomainService;

    public UpdateReleasesWhenVariableValueChanges(IUnitOfWork unitOfWork,
        VariableSetDomainService variableSetDomainService)
    {
        _variableSetDomainService = Guards.HasValue(variableSetDomainService, nameof(variableSetDomainService));
        _unitOfWork = Guards.HasValue(unitOfWork, nameof(unitOfWork));
    }

    public async Task ExecuteAsync(VariableValueSetEvent evt, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Variable changed: " + evt);

        // TODO: super hack. inefficient, but a start... get all releases that use the variable set

        // find all releases that use the variable.
        //  NOT DONE - need to work through that. for now, test every release.
        //  to do:
        //    1 - the release needs a copy of the variable set as-of when the release was created
        //    2 - need to calculate all used variables for the release, including nested.
        //        either calculate and save it to the aggregate (doesn't seem right),
        //        of calculate on demand from the values stored in #1.
        // recalculate the value of the release using the release's value, and the new variable.
        // see if the value has changed, and report it to the aggregate.
        // the aggregate will then decide what to do about it.
        // for example: a variable may be value xyz, then 123, then xyz.
        // the value is current, then out of date, then current.
        // thus, we don't report only when the value has changed;
        // we report it either way so the aggregate can update it's state.
        // todo: consider how much of this logic should be moved into the aggregate.

        var vsAggregate =
            await _unitOfWork.VariableSets.FindOneAsync(new VariableSetNameIs(evt.VariableSetName), cancellationToken);
        var variableSet =
            await _variableSetDomainService.GetVariableSetComposedAsync(evt.VariableSetName, cancellationToken);
        var sections = await _unitOfWork.Sections.FindAsync(new All(), cancellationToken);

        // get the composer for all variable sets
        var values = variableSet.ToValueDictionary();
        foreach (var section in sections)
        {
            // find all environments of the variable set's environment type
            var environments = section
                .Environments
                .Where(e => e.EnvironmentType.Equals(
                    vsAggregate.EnvironmentType, StringComparison.OrdinalIgnoreCase));
            foreach (var env in environments)
            {
                // find all releases using the variable set
                var releases = env.Releases.Where(r => r.VariableSetId == vsAggregate.Id);
                foreach (var release in releases)
                {
                    var resolved = await JsonUtility.ResolveAsync(release.ModelValue.ToJsonNetJson(), values,
                        cancellationToken);
                    var changed = !JToken.DeepEquals(resolved, release.ResolvedValue.ToJsonNetJson());
                    section.SetReleaseValueChanged(env.Id, release.Id, changed);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}