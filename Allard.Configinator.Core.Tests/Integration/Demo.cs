using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Xunit;

namespace Allard.Configinator.Core.Tests.Integration;

public class Demo
{
    private readonly IUnitOfWork _uow;
    
    [Fact]
    public void Blah()
    {
        var variables = new VariableSetAggregate(NewVariableSetId(10), "test1");
        var section = new SectionAggregate(NewSectionId(0), "blah", "path");
        
    }
}