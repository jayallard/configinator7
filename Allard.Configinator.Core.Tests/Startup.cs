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
            .AddSingleton<EnvironmentValidationService>()
            .AddTransient<NamespaceDomainService>()

            // event handlers - HACK
            .AddScoped<IEventHandler<VariableValueSetEvent>, UpdateReleasesWhenVariableValueChanges>()
            .AddScoped<IEventHandler<SchemaCreatedEvent>, SchemaNamespaceHandler>()
            .AddScoped<IEventHandler<SectionCreatedEvent>, SectionNamespaceHandler>()
            .AddScoped<IEventHandler<VariableSetCreatedEvent>, VariableSetNamespaceHandler>()

            // database
            .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
            .AddSingleton<ISectionRepository, SectionRepositoryMemory>()
            .AddSingleton<IVariableSetRepository, VariableSetRepositoryMemory>()
            .AddSingleton<ISchemaRepository, SchemaRepositoryMemory>()
            .AddSingleton<INamespaceRepository, NamespaceRepositoryMemory>();

        // hack
        var environmentRules = new EnvironmentRules();
        host.Configuration.Bind("environmentRules", environmentRules);
        services.AddSingleton(environmentRules);
    }
}