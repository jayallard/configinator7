using System;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.DomainServices;

public class VariableSetDomainServiceTests
{
    private readonly VariableSetDomainService _service;

    public VariableSetDomainServiceTests(VariableSetDomainService service)
    {
        _service = service;
    }

    [Fact]
    public async Task VariableSetNameMustBeUnique()
    {
        await _service.CreateVariableSetAsync("/ns", "ABC", "development");
        var test = () => _service.CreateVariableSetAsync("/ns", "abc", "staging");
        await test.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The variable set name is already in use. Variable Set Name=abc");
    }

    [Fact]
    public async Task EnvironmentTypeMustExist()
    {
        var test = () => _service.CreateVariableSetAsync("/ns", "abc", "boom");
        await test.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Environment type doesn't exist: boom");
    }

    [Fact]
    public async Task OverrideMustBeInSameNamespaceOrAscendant()
    {
        await _service.CreateVariableSetAsync("/ns", "ABC", "development");
        var test = () => _service.CreateVariableSetOverride("/boom", "xyz", "abc");
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The base variable set must be in an self/ascendant namespace of the override.\nVariable Set=/ns, ABC\nOverride Set=/boom, xyz");
    }
}