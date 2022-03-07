using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainEventHandlers;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Deployer.Memory;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using ConfiginatorWeb.Queries;
using MediatR;
using Newtonsoft.Json.Linq;
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
    .AddTransient<SchemaDomainService>()
    .AddSingleton<EnvironmentDomainService>()
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
    .AddSingleton<INamespaceRepository, NamespaceRepositoryMemory>()

    // queries
    .AddTransient<ISectionQueries, SectionQueriesCoreRepository>()
    .AddTransient<IVariableSetQueries, VariableSetQueriesCoreRepository>()
    .AddTransient<ISchemaQueries, SchemaQueriesCoreRepository>()
    .AddTransient<INamespaceQueries, NamespaceQueriesCoreRepository>()

    // deployment
    // NOTE: deployer factory doesn't actually do anything in this case.
    .AddSingleDeployer<MemoryDeployer>()
    .AddTransient<IConfigurationProvider, HardCodedConfigurationProvider>()
    .AddSingleton<MemoryConfigurationStore>();

// hack
var environmentRules = new EnvironmentRules();
builder.Configuration.Bind("environmentRules", environmentRules);
builder.Services.AddSingleton(environmentRules);

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
    "default",
    "{controller=Home}/{action=Index}/{id?}");


using var scope = app.Services.CreateScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

var sectionService = scope.ServiceProvider.GetRequiredService<SectionDomainService>();
var variableSetService = scope.ServiceProvider.GetRequiredService<VariableSetDomainService>();
var schemaService = scope.ServiceProvider.GetRequiredService<SchemaDomainService>();

// put the shared schema in the root so that everything can get to it.
var kafkaSchema = await schemaService.CreateSchemaAsync(null, "/", new SchemaName("/ppm/kafka/1.0.0"), null,
    await GetSchema("__kafka-1.0.0.json"));
await schemaService.PromoteSchemaAsync(kafkaSchema.SchemaName, "staging");


var variableSetEntity = await variableSetService.CreateVariableSetAsync("demo", "variables1", "development");
variableSetEntity.SetValue("first", "Santa");
variableSetEntity.SetValue("last", "Claus");

var variableSet2Entity = await variableSetService.CreateVariableSetOverride("demo", "variables2", "variables1");
variableSet2Entity.SetValue("first", "SANTA!!!");

await variableSetService.CreateVariableSetOverride("demo", "variables2a", "variables2");
await variableSetService.CreateVariableSetOverride("demo", "variables3", "variables2");
await variableSetService.CreateVariableSetOverride("demo", "variables4", "variables3");
await variableSetService.CreateVariableSetOverride("demo", "variables5a", "variables4");
await variableSetService.CreateVariableSetOverride("demo", "variables5b", "variables4");
await variableSetService.CreateVariableSetOverride("demo", "variablesAB", "variables3");
await variableSetService.CreateVariableSetOverride("demo", "yabba", "variablesAB");
await variableSetService.CreateVariableSetOverride("demo", "dabbadoo", "variablesAB");

await variableSetService.CreateVariableSetAsync("demo", "root2", "staging");
await variableSetService.CreateVariableSetOverride("demo", "blah1", "root2");
await variableSetService.CreateVariableSetOverride("demo", "blah2", "root2");
await variableSetService.CreateVariableSetOverride("demo", "c1", "blah2");

// -------------------------------------
// UAL Dev Variable Set
// -------------------------------------

var ualDevVariableSet = await variableSetService.CreateVariableSetAsync("/ual", "ual-variables", "development");
ualDevVariableSet.SetValue("kafka.bootstrapservers", "localhost:9091");
ualDevVariableSet.SetValue("kafka.username", "kuser");
ualDevVariableSet.SetValue("kafka.password", "kpassword");
ualDevVariableSet.SetValue("redshift.connectionstring", "redshift");
ualDevVariableSet.SetValue("postgres.connectionstring",
    "Database=SomethingCool;Server=TheCloud&UserId=$$postgres.username$$;Password=$$postgres.password$$");
ualDevVariableSet.SetValue("postgres.username", "PG-USER");
ualDevVariableSet.SetValue("postgres.password", "PG-PASSWORD");
var kafka = new Dictionary<string, object>
{
    {"bootstrapservers", "$$kafka.bootstrapservers$$"},
    {"username", "$$kafka.username$$"},
    {"password", "$$kafka.password$$"}
};
ualDevVariableSet.SetValue("kafka", JObject.FromObject(kafka));

// -------------------------------------
// MERGE
// -------------------------------------
var mergeValue = await GetValue("merge-service.json");

var mergeSection = await sectionService.CreateSectionAsync("/ual/merge-service", "merge-service");
await sectionService.PromoteToEnvironmentType(mergeSection, "Staging");

var mergeDev = await sectionService.AddEnvironmentToSectionAsync(mergeSection, "development");
await sectionService.AddEnvironmentToSectionAsync(mergeSection, "development-jay");
var mergeSchema =
    await schemaService.CreateSchemaAsync(mergeSection.Id, "/ual/merge-service",
        new SchemaName("ual-merge-service/1.0.0"), null,
        await GetSchema("ual-merge-service-1.0.0.json"));
await schemaService.PromoteSchemaAsync(mergeSchema.SchemaName, "staging");


await sectionService.CreateReleaseAsync(mergeSection, mergeDev.Id, ualDevVariableSet.Id, mergeSchema.Id, mergeValue,
    CancellationToken.None);
// mergeSection.SetDeployed(mergeDev.Id, mergeRelease.Id, await idService.GetIdAsync<DeploymentId>(),
//     new DeploymentResult(true, new List<DeploymentResultMessage>().AsReadOnly()), DateTime.Now,
//     "Initial Setup - from code");

// -------------------------------------
// INGESTION
// -------------------------------------
var ingestionSection = await sectionService.CreateSectionAsync("/ual/ingestion-service", "ingestion-service");

var ingestionSchema = await schemaService.CreateSchemaAsync(ingestionSection.Id, "/ual/ingestion-service",
    new SchemaName("ual-ingestion-service/1.0.0"), null,
    await GetSchema("ual-ingestion-service-1.0.0.json"));
var ingestionValue = await GetValue("ingestion-service.json");
await sectionService.AddEnvironmentToSectionAsync(ingestionSection, "development");
var ingestionRelease = await sectionService.CreateReleaseAsync(
    ingestionSection,
    ingestionSection.Environments.Single().Id,
    ualDevVariableSet.Id,
    ingestionSchema.Id,
    ingestionValue);
ingestionSection.SetDeployed(new DeploymentId(33333),
    ingestionRelease.Id,
    ingestionSection.Environments.Single().Id,
    new DeploymentResult(true, new List<DeploymentResultMessage>().AsReadOnly()), DateTime.Now, "fake deploy");

await uow.SaveChangesAsync();

app.Run();

async Task<JsonDocument> GetSchema(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "SetupData", "Schemas", fileName);
    var json = await File.ReadAllTextAsync(f);
    return JsonDocument.Parse(json);
}

async Task<JsonDocument> GetValue(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "SetupData", "Values", fileName);
    var json = await File.ReadAllTextAsync(f);
    return JsonDocument.Parse(json);
}