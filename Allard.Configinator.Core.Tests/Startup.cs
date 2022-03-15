using Allard.Configinator.Core.DomainEventHandlers;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Allard.Configinator.Core.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureHostConfiguration(b => b
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables());
    }
    public void ConfigureServices(IServiceCollection services, HostBuilderContext host)
    {
        // infrastructure miscellaneous
        services
            .AddScoped<IEventPublisher, MediatorPublisher>()
            .AddSingleton<IIdentityService, IdentityServiceMemory>()
            .AddMediatR(typeof(Program))
            .AddTransient<SchemaLoader>()

            // domain services
            .AddTransient<SectionDomainService>()
            .AddTransient<VariableSetDomainService>()
            .AddTransient<SchemaDomainService>()
            .AddSingleton<EnvironmentDomainService>()
            .AddTransient<NamespaceDomainService>()

            // event handlers - HACK
            .AddScoped<IDomainEventHandler<VariableValueSetEvent>, UpdateReleasesWhenVariableValueChanges>()
            .AddScoped<IDomainEventHandler<SchemaCreatedEvent>, SchemaNamespaceHandler>()
            .AddScoped<IDomainEventHandler<SectionCreatedEvent>, SectionNamespaceHandler>()
            .AddScoped<IDomainEventHandler<VariableSetCreatedEvent>, VariableSetNamespaceHandler>()

            // database
            .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
            .AddScoped<ISectionRepository, SectionRepositoryMemory>()
            .AddScoped<IVariableSetRepository, VariableSetRepositoryMemory>()
            .AddScoped<ISchemaRepository, SchemaRepositoryMemory>()
            .AddScoped<INamespaceRepository, NamespaceRepositoryMemory>();

        // hack
        var environmentRules = new EnvironmentRules();
        host.Configuration.Bind("environmentRules", environmentRules);
        services.AddSingleton(environmentRules);
    }
}