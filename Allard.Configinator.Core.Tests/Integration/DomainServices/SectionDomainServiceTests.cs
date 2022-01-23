﻿using System;
using System.Threading.Tasks;
using Allard.Configinator.Core.DomainServices;
using FluentAssertions;
using Xunit;

namespace Allard.Configinator.Core.Tests.Integration.DomainServices;

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
        await _domainService.CreateSectionAsync("name", "path");
        var test = async () => await _domainService.CreateSectionAsync("name", "path");
        await test.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Section already exists: name");
    }

    [Fact]
    public async Task ThrowExceptionIfPathExists()
    {
        await _domainService.CreateSectionAsync("name1", "path");
        var test = async () => await _domainService.CreateSectionAsync("name2", "path");
        await test.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("The path is already in use by another section");
    }
}