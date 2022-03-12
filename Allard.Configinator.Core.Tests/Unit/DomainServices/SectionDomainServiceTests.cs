using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.DomainServices;

public class SectionDomainServiceTests
{
    private readonly SectionDomainService _domainService;

    public SectionDomainServiceTests(SectionDomainService domainService)
    {
        _domainService = domainService;
    }

    [Fact]
    public async Task ThrowExceptionIfNameExists()
    {
        await _domainService.CreateSectionAsync("/ns", "name");
        var test = async () => await _domainService.CreateSectionAsync("/ns", "name");
        await test
            .Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Section already exists: name");
    }

    /// <summary>
    /// Example: dev -> staging -> production
    /// If the section is in only DEV it can't be promoted
    /// directly to production.
    /// </summary>
    [Fact]
    public async Task CantPromoteToEnvironmentTypeOutOfOrder()
    {
        var section = await _domainService.CreateSectionAsync("/ns", "section");
        var test = () => _domainService.PromoteToEnvironmentType(section, "Production");
        await test
            .Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("Section can't be promoted. Section Name=section, Target Environment Type=Production");
    }

    /// <summary>
    /// If the environment doesn't exist in configinator, it can't
    /// be added to a section.
    /// </summary>
    [Fact]
    public async Task CantAddEnvironmentToSectionIfEnvironmentDoesntExist()
    {
        var section = await _domainService.CreateSectionAsync("/ns", "section");
        var test = async () => await _domainService.AddEnvironmentToSectionAsync(section, "go-boom");
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The environment doesn't exist: go-boom");
    }

    /// <summary>
    /// If the section doesn't support an environment type,
    /// you cant add an environment of tha type.
    /// IE: the section is DEVELOPMENT, but the environment is STAGING.
    /// The environment can't be added.
    /// </summary>
    [Fact]
    public async Task CantAddEnvironmentIfEnvironmentTypeDoesntExistInSection()
    {
        var section = await _domainService.CreateSectionAsync("/ns", "section");
        var test = async () => await _domainService.AddEnvironmentToSectionAsync(section, "Staging");
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The section doesn't support the staging environment type.");
    }
}