using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Allard.Configinator.Infrastructure.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddScoped<INamespaceRepository, NamespaceRepositoryMemory>()
            .AddScoped<ISectionRepository, SectionRepositoryMemory>()
            .AddScoped<IVariableSetRepository, VariableSetRepositoryMemory>()
            .AddScoped<ISchemaRepository, SchemaRepositoryMemory>()
            .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
            .AddScoped<IIdentityService, IdentityServiceMemory>()
            .AddScoped<SectionDomainService>()
            .AddScoped<IEventPublisher, MediatorPublisher>()
            .AddMediatR(typeof(Startup));
    }
}