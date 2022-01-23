using Allard.Configinator.Core.Model;
using Allard.DomainDrivenDesign;

namespace Allard.Configinator.Core.Specifications;

public class PathIs : ISpecification<SectionEntity>
{
    private readonly string _path;

    public PathIs(string path)
    {
        _path = path;
    }

    public bool IsSatisfied(SectionEntity obj) =>
        obj.Path.Equals(_path, StringComparison.OrdinalIgnoreCase);
}