using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Specifications;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Repositories;

public interface IUnitOfWork
{
    IDataChangeTracker<SectionAggregate, SectionId> Sections { get; }
    IDataChangeTracker<TokenSetAggregate, TokenSetId> TokenSets { get; }
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

    public static Task<TokenSetAggregate> GetTokenSetAsync(this IDataChangeTracker<TokenSetAggregate, TokenSetId> tracker,
        string tokenSetName, CancellationToken cancellationToken = default) =>
        tracker.FindOneAsync(new TokenSetNameIs(tokenSetName), cancellationToken);

    public static async Task<TokenSetAggregate?> GetTokenSetAsyncIfNotNull(this IDataChangeTracker<TokenSetAggregate, TokenSetId> tracker,
        string? tokenSetName,
        CancellationToken cancellationToken = default) =>
        tokenSetName == null 
            ? null 
            : await tracker.GetTokenSetAsync(tokenSetName, cancellationToken);

}