using Allard.Configinator.Core.Model;

namespace Allard.Configinator.Infrastructure;

public interface IDatabase
{
    IDictionary<SectionId, SectionEntity> Sections { get; }
}