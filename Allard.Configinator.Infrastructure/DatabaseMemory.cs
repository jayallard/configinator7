using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Infrastructure;

public class DatabaseMemory
{
    public Dictionary<SectionId, SectionEntity> Sections { get; } = new();
}