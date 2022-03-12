using System;
using System.Linq;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Specifications;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.DomainServices;

public class NamespaceDomainServiceTests
{
    private readonly NamespaceDomainService _service;
    private readonly IUnitOfWork _unitOfWork;
    public NamespaceDomainServiceTests(NamespaceDomainService service, IUnitOfWork unitOfWork)
    {
        _service = service;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get the namespace if it exists;
    /// create it if it doesn't.
    /// </summary>
    [Fact]
    public async Task GetOrCreateNamespace()
    {
        (await _unitOfWork.Namespaces.FindAsync(new All())).Should().BeEmpty();
        var ns = await _service.GetOrCreateAsync("/boo");
        (await _unitOfWork.Namespaces.FindAsync(new All())).Count.Should().Be(1);
        
        var ns2 = await _service.GetOrCreateAsync("/boo");
        (await _unitOfWork.Namespaces.FindAsync(new All())).Count.Should().Be(1);
        ns2.Id.Id.Should().Be(ns.Id.Id);
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("/abc//xyz", false)]
    [InlineData("/abc/de-f/x.yz", true)]
    [InlineData("/ab\\c/de-f/x.yz", false)]
    [InlineData("/abc/def/xyz/", false)]
    public async Task EnsureValidNamespaceName(string? @namespace, bool isValid)
    {
        if (isValid)
        {
            NamespaceDomainService.EnsureValidNameSpace(@namespace!);
            return;
        }

        var test = () => NamespaceDomainService.EnsureValidNameSpace(@namespace!);
        test.Should().Throw<InvalidOperationException>();
    }
}