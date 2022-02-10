using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    IDataChangeTracker<SectionAggregate, SectionId> Sections { get; }
    IDataChangeTracker<VariableSetAggregate, VariableSetId> VariableSets { get; }
    IDataChangeTracker<GlobalSchemaAggregate, GlobalSchemaId> GlobalSchemas { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public static class UnitOfWorkExtensionMethods
{
    public static Task<SectionAggregate> GetSectionAsync(this IDataChangeTracker<SectionAggregate, SectionId> tracker,
        string sectionName, CancellationToken cancellationToken = default) =>
        tracker.FindOneAsync(new SectionNameIs(sectionName), cancellationToken);

    public static Task<bool> Exists(this IDataChangeTracker<SectionAggregate, SectionId> tracker,
        string sectionName, CancellationToken cancellationToken = default) =>
        tracker.Exists(new SectionNameIs(sectionName));

    public static Task<VariableSetAggregate> GetVariableSetAsync(this IDataChangeTracker<VariableSetAggregate, VariableSetId> tracker,
        string variableSetName, CancellationToken cancellationToken = default) =>
        tracker.FindOneAsync(new VariableSetNameIs(variableSetName), cancellationToken);

    public static async Task<VariableSetAggregate?> GetVariableSetIfNotNullAsync(this IDataChangeTracker<VariableSetAggregate, VariableSetId> tracker,
        string? variableSetName,
        CancellationToken cancellationToken = default) =>
        variableSetName == null 
            ? null 
            : await tracker.GetVariableSetAsync(variableSetName, cancellationToken);

}