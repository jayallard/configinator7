using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Infrastructure;

public class DatabaseMemory
{
    public Dictionary<long, SectionAggregate> Sections { get; }= new();
}