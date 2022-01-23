using System;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Allard.Configinator.Core.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISectionRepository, SectionRepositoryMemory>();
        services.AddScoped<IUnitOfWork, UnitOfWorkMemory>();
        services.AddSingleton<DatabaseMemory>();
        services.AddScoped<IUnitOfWork, UnitOfWorkMemory>();
        services.AddScoped<IIdentityService, IdentityServiceMemory>();
        services.AddScoped<SectionDomainService>();
        services.AddScoped<IEventPublisher, MediatorPublisher>();
        services.AddMediatR(typeof(Startup));
    }
}