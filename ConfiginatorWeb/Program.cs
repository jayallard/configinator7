using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainEventHandlers;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Deployer.Abstractions;
using Allard.Configinator.Deployer.Memory;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using Allard.Json;
using ConfiginatorWeb.Queries;
using MediatR;
using NuGet.Versioning;
using IConfigurationProvider = Allard.Configinator.Deployer.Memory.IConfigurationProvider;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    // infrastructure miscellaneous
    .AddScoped<IEventPublisher, MediatorPublisher>()
    .AddSingleton<IIdentityService, IdentityServiceMemory>()
    .AddMediatR(typeof(Program))
    .AddTransient<SchemaLoader>()

    // domain services
    .AddTransient<SectionDomainService>()
    .AddTransient<VariableSetDomainService>()
    .AddTransient<GlobalSchemaDomainService>()

    // event handlers - HACK
    .AddScoped<IEventHandler<VariableValueSetEvent>, UpdateReleasesWhenVariableValueChanges>()

    // database
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddSingleton<ISectionRepository, SectionRepositoryMemory>()
    .AddSingleton<IVariableSetRepository, VariableSetRepositoryMemory>()
    .AddSingleton<IGlobalSchemaRepository, GlobalSchemaRepositoryMemory>()

    // queries
    .AddTransient<ISectionQueries, SectionQueriesCoreRepository>()
    .AddTransient<IVariableSetQueries, VariableSetQueriesCoreRepository>()
    .AddTransient<IGlobalSchemaQueries, GlobalSchemaQueriesCoreRepository>()

    // deployment
    .AddTransient<IDeployerFactory, MemoryDeployerFactory>()
    .AddTransient<IConfigurationProvider, HardCodedConfigurationProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using var scope = app.Services.CreateScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

var sectionService = scope.ServiceProvider.GetRequiredService<SectionDomainService>();
var variableSetService = scope.ServiceProvider.GetRequiredService<VariableSetDomainService>();
var globalSchemas = scope.ServiceProvider.GetRequiredService<GlobalSchemaDomainService>();

await globalSchemas.CreateGlobalSchemaAsync("/ppm/kafka/1.0.0", "Kafka config", await GetSchema("__kafka-1.0.0.json"));


var variableSetEntity = await variableSetService.CreateVariableSetAsync("tokens1");
variableSetEntity.SetValue("first", "Santa");
variableSetEntity.SetValue("last", "Claus");

var variableSet2Entity = await variableSetService.CreateVariableSetAsync("tokens2", "tokens1");
variableSet2Entity.SetValue("first", "SANTA!!!");


await variableSetService.CreateVariableSetAsync("tokens2a", "tokens2");
await variableSetService.CreateVariableSetAsync("tokens3", "tokens2");
await variableSetService.CreateVariableSetAsync("tokens4", "tokens3");
await variableSetService.CreateVariableSetAsync("tokens5a", "tokens4");
await variableSetService.CreateVariableSetAsync("tokens5b", "tokens4");

await variableSetService.CreateVariableSetAsync("tokensAB", "tokens3");
await variableSetService.CreateVariableSetAsync("yabba", "tokensAB");
await variableSetService.CreateVariableSetAsync("dabbadoo", "tokensAB");


await variableSetService.CreateVariableSetAsync("root2", null);
await variableSetService.CreateVariableSetAsync("blah1", "root2");
await variableSetService.CreateVariableSetAsync("blah2", "root2");
await variableSetService.CreateVariableSetAsync("c1", "blah2");
await variableSetService.CreateVariableSetAsync("c2", "blah2");


var modelValue =
    JsonDocument.Parse(
        "{ \"firstName\": \"$$first$$\", \"lastName\": \"$$last$$\", \"age\": 44, \"kafka\": { \"brokers\": \"b\", \"user\": \"u\", \"password\": \"p\" } }");
var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await sectionService.CreateSectionAsync("name1", "path1");

var env1 = section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev");
section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev2");
var schema1 =
    await sectionService.AddSchemaToSectionAsync(section1, new SemanticVersion(1, 0, 0), await GetSchema("1.0.0.json"));

await sectionService.AddSchemaToSectionAsync(section1, new SemanticVersion(2, 0, 0), await GetSchema("2.0.0.json"));

var release = await sectionService.CreateReleaseAsync(section1, env1.Id, variableSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now, "Initial Setup - from code");

await sectionService.CreateReleaseAsync(section1, env1.Id, variableSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now, "Initial Setup - from code");

await sectionService.CreateSectionAsync("name2", "path2");
await uow.SaveChangesAsync();

app.Run();

async Task<JsonDocument> GetSchema(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", fileName);
    var json = await File.ReadAllTextAsync(f);
    return JsonDocument.Parse(json);
}
