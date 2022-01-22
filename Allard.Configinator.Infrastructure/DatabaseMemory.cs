using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Infrastructure;

public class DatabaseMemory : IDatabase
{
    public IDictionary<SectionId, SectionEntity> Sections { get; } = new Dictionary<SectionId, SectionEntity>();
}