using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Services;
using Allard.Configinator.Core.Services.Revisit;
using Allard.Configinator.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Allard.Configinator.Core.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISectionRepository, SectionRepositoryMemory>();
        services.AddScoped<IUnitOfWork, UnitOfWorkMemory>();
        services.AddSingleton<IIdService, IdServiceMemory>();
        services.AddSingleton<DatabaseMemory>();
        services.AddScoped<IUnitOfWork, UnitOfWorkMemory>();
    }
}