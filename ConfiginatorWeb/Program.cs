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
    .AddSingleton<INamespaceRepository, NamespaceRepositoryMemory>()

    // queries
    .AddTransient<ISectionQueries, SectionQueriesCoreRepository>()
    .AddTransient<IVariableSetQueries, VariableSetQueriesCoreRepository>()
    .AddTransient<ISchemaQueries, SchemaQueriesCoreRepository>()

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

var kafkaSchema = await schemaService.CreateSchemaAsync("/ual/merge-service", new SchemaName("/ppm/kafka/1.0.0"), null, "Kafka config",
    await GetSchema("__kafka-1.0.0.json"));
await uow.Schemas.AddAsync(kafkaSchema);
await schemaService.PromoteSchemaAsync(kafkaSchema.SchemaName, "staging");


var variableSetEntity = await variableSetService.CreateVariableSetAsync("ns", "variables1", "development");
await uow.VariableSets.AddAsync(variableSetEntity);
variableSetEntity.SetValue("first", "Santa");
variableSetEntity.SetValue("last", "Claus");

var variableSet2Entity = await variableSetService.CreateVariableSetOverride("ns", "variables2", "variables1");
variableSet2Entity.SetValue("first", "SANTA!!!");
await uow.VariableSets.AddAsync(variableSet2Entity);

await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variables2a", "variables2"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variables3", "variables2"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variables4", "variables3"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variables5a", "variables4"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variables5b", "variables4"));

await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "variablesAB", "variables3"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "yabba", "variablesAB"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "dabbadoo", "variablesAB"));

await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetAsync("ns", "root2", "staging"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "blah1", "root2"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "blah2", "root2"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "c1", "blah2"));
await uow.VariableSets.AddAsync(await variableSetService.CreateVariableSetOverride("ns", "c2", "blah2"));


var modelValue =
    JsonDocument.Parse(
        "{ \"firstName\": \"$$first$$\", \"lastName\": \"$$last$$\", \"age\": 44, \"kafka\": { \"brokers\": \"b\", \"user\": \"u\", \"password\": \"p\" } }");
var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await sectionService.CreateSectionAsync("data-domain","merge-service");
await uow.Sections.AddAsync(section1);

var env1 = await sectionService.AddEnvironmentToSectionAsync(section1, "development");
await sectionService.AddEnvironmentToSectionAsync(section1, "development-jay");
var schema1 =
    await schemaService.CreateSchemaAsync("/ual/ingestion-service", new SchemaName("section1/1.0.0"), section1.Id, null,
        await GetSchema("1.0.0.json"));
await uow.Schemas.AddAsync(schema1);
await schemaService.PromoteSchemaAsync(schema1.SchemaName, "staging");

// this revealed a bug... need to add the entities to the schema within the service,
// not the app code. otherwise, the user might add some entities and not others.
// then, not everything is saved. (example: remove this AddAsync. Then there are entities referencing this schema,
// but this schema doesn't save to the db
var schema2 = await schemaService.CreateSchemaAsync("/ual/ingestion-service", new SchemaName("section1/2.0.0"), section1.Id, null,
    await GetSchema("2.0.0.json"));
await uow.Schemas.AddAsync(schema2);

var release = await sectionService.CreateReleaseAsync(section1, env1.Id, variableSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(),
    new DeploymentResult(true, new List<DeploymentResultMessage>().AsReadOnly()), DateTime.Now,
    "Initial Setup - from code");

await sectionService.CreateReleaseAsync(section1, env1.Id, variableSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(),
    new DeploymentResult(true, new List<DeploymentResultMessage>().AsReadOnly()), DateTime.Now,
    "Initial Setup - from code");

var section2 = await sectionService.CreateSectionAsync("data-domain","ingestion-service");
await uow.Sections.AddAsync(section2);
await uow.SaveChangesAsync();

app.Run();

async Task<JsonDocument> GetSchema(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", fileName);
    var json = await File.ReadAllTextAsync(f);
    return JsonDocument.Parse(json);
}