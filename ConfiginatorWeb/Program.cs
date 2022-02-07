using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainEventHandlers;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Core.Schema;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using Allard.Json;
using ConfiginatorWeb.Queries;
using MediatR;
using NuGet.Versioning;

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
    .AddTransient<TokenSetDomainService>()
    .AddTransient<GlobalSchemaDomainService>()

    // event handlers - HACK
    .AddScoped<IEventHandler<TokenValueSetEvent>, UpdateReleasesWhenTokenValueChanges>()

    // database
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddSingleton<ISectionRepository, SectionRepositoryMemory>()
    .AddSingleton<ITokenSetRepository, TokenSetRepositoryMemory>()
    .AddSingleton<IGlobalSchemaRepository, GlobalSchemaRepositoryMemory>()

    // queries
    .AddTransient<ISectionQueries, SectionQueriesCoreRepository>()
    .AddTransient<ITokenSetQueries, TokenSetQueriesCoreRepository>()
    .AddTransient<IGlobalSchemaQueries, GlobalSchemaQueriesCoreRepository>();

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
var tokenService = scope.ServiceProvider.GetRequiredService<TokenSetDomainService>();
var globalSchemas = scope.ServiceProvider.GetRequiredService<GlobalSchemaDomainService>();

await globalSchemas.CreateGlobalSchemaAsync("/ppm/kafka/1.0.0", "Kafka config", await GetSchema("__kafka-1.0.0.json"));


var tokenSetEntity = await tokenService.CreateTokenSetAsync("tokens1");
tokenSetEntity.SetValue("first", "Santa");
tokenSetEntity.SetValue("last", "Claus");

var tokenSet2Entity = await tokenService.CreateTokenSetAsync("tokens2", "tokens1");
tokenSet2Entity.SetValue("first", "SANTA!!!");

var modelValue =
    JsonDocument.Parse(
        "{ \"firstName\": \"$$first$$\", \"lastName\": \"$$last$$\", \"age\": 44, \"kafka\": { \"brokers\": \"b\", \"user\": \"u\", \"password\": \"p\" } }");
var composed = new TokenSetComposer(new[] {tokenSetEntity.ToTokenSet()}).Compose("tokens1");

var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await sectionService.CreateSectionAsync("name1", "path1");

var env1 = section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev");
section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev2");
var schema1 =
    await sectionService.AddSchemaToSectionAsync(section1, new SemanticVersion(1, 0, 0), await GetSchema("1.0.0.json"));

await sectionService.AddSchemaToSectionAsync(section1, new SemanticVersion(2, 0, 0), await GetSchema("2.0.0.json"));

var release = await sectionService.CreateReleaseAsync(section1, env1.Id, tokenSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now);

await sectionService.CreateReleaseAsync(section1, env1.Id, tokenSetEntity.Id, schema1.Id, modelValue,
    CancellationToken.None);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now);

await sectionService.CreateSectionAsync("name2", "path2");
await uow.SaveChangesAsync();

app.Run();

async Task<JsonDocument> GetSchema(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", fileName);
    var json = await File.ReadAllTextAsync(f);
    return JsonDocument.Parse(json);
}
