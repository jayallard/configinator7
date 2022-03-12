using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Unit.DomainServices;

public class SectionDomainServiceTests
{
    private readonly SectionDomainService _sectionService;
    private readonly SchemaDomainService _schemaService;
    private readonly VariableSetDomainService _variableSetDomainService;

    public SectionDomainServiceTests(SectionDomainService sectionService, SchemaDomainService schemaService, VariableSetDomainService variableSetDomainService)
    {
        _sectionService = sectionService;
        _schemaService = schemaService;
        _variableSetDomainService = variableSetDomainService;
    }

    [Fact]
    public async Task ThrowExceptionIfNameExists()
    {
        await _sectionService.CreateSectionAsync("/ns", "name");
        var test = async () => await _sectionService.CreateSectionAsync("/ns", "name");
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
        var section = await _sectionService.CreateSectionAsync("/ns", "section");
        var test = () => _sectionService.PromoteToEnvironmentType(section, "Production");
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
        var section = await _sectionService.CreateSectionAsync("/ns", "section");
        var test = async () => await _sectionService.AddEnvironmentToSectionAsync(section, "go-boom");
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
        var section = await _sectionService.CreateSectionAsync("/ns", "section");
        var test = async () => await _sectionService.AddEnvironmentToSectionAsync(section, "Staging");
        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The section doesn't support the staging environment type.");
    }

    /// <summary>
    /// dev -> staging -> production
    /// You can't create a STAGING release if the
    /// schema isn't assigned to STAGING.
    /// </summary>
    [Fact]
    public async Task SchemaMustBeAvailableForEnvironmentToCreateRelease()
    {
        var section = await _sectionService.CreateSectionAsync("/ns", "section");
        await _sectionService.PromoteToEnvironmentType(section, "staging");
        await _sectionService.AddEnvironmentToSectionAsync(section, "staging");

        var schema = await _schemaService.CreateSchemaAsync(
            section.Id,
            "/ns",
            new SchemaName("abc/1.0.0"),
            null,
            TestSchema());

        // will fail because the release is staging,
        // but the schema hasn't been promoted to staging.
        var test = async () => await _sectionService.CreateReleaseAsync(
            section,
            section.GetEnvironment("staging").Id,
            null,
            schema.Id,
            TestValue());

        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "The schema doesn't belong to the release's environment type. Schema=abc/1.0.0, Target Environment Type=staging");
    }

    [Fact]
    public async Task VariableSetMustBeOfTheReleasesEnvironmentType()
    {
        var section = await _sectionService.CreateSectionAsync("/ns", "section");
        await _sectionService.AddEnvironmentToSectionAsync(section, "development");
        await _sectionService.PromoteToEnvironmentType(section, "staging");
        await _sectionService.AddEnvironmentToSectionAsync(section, "staging");

        var schema = await _schemaService.CreateSchemaAsync(
            section.Id,
            "/ns",
            new SchemaName("abc/1.0.0"),
            null,
            TestSchema());

        var variableSet = await _variableSetDomainService.CreateVariableSetAsync(
            "/ns",
            "name",
            "staging");

        // will fail because the release is staging,
        // but the schema hasn't been promoted to staging.
        var test = async () => await _sectionService.CreateReleaseAsync(
            section,
            section.GetEnvironment("development").Id,
            variableSet.Id,
            schema.Id,
            TestValue());

        await test
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("The variable set isn't the same environment type as the release");
    }
}