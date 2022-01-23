﻿using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Infrastructure;

public class SectionRepositoryMemory : ISectionRepository
{
    private readonly DatabaseMemory _database;

    public SectionRepositoryMemory(DatabaseMemory database)
    {
        _database = database;
    }

    public Task<SectionEntity?> GetAsync(SectionId id, CancellationToken cancellationToken)
    {
        var section = (SectionEntity?)_database.Sections[id];
        return Task.FromResult(section);
    }

    public Task<IEnumerable<SectionEntity>> FindAsync(ISpecification<SectionEntity> specification)
    {
        return Task.FromResult(_database.Sections.Values.Where(specification.IsSatisfied));
    }

    public Task<bool> Exists(ISpecification<SectionEntity> specification)
    {
        return Task.FromResult(_database.Sections.Values.Any(specification.IsSatisfied));
    }

    public Task SaveAsync(SectionEntity section)
    {
        _database.Sections[section.Id] = section;
        return Task.CompletedTask;
    }
}