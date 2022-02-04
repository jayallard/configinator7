using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Allard.Configinator.Core.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddScoped<ISectionRepository, SectionRepositoryMemory>()
            .AddScoped<ITokenSetRepository, TokenSetRepositoryMemory>()
            .AddScoped<IGlobalSchemaRepository, GlobalSchemaRepositoryMemory>()
            .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
            .AddScoped<IIdentityService, IdentityServiceMemory>()
            .AddScoped<SectionDomainService>()
            .AddScoped<IEventPublisher, MediatorPublisher>()
            .AddMediatR(typeof(Startup));
}