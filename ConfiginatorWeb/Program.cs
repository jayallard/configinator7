using System.Text.Json;
using Allard.Configinator.Core;
using Allard.Configinator.Core.DomainEventHandlers;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Infrastructure;
using Allard.Configinator.Infrastructure.Repositories;
using Allard.DomainDrivenDesign;
using Allard.Json;
using ConfiginatorWeb.Queries;
using MediatR;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.References;
using NuGet.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    // infrastructure miscellaneous
    .AddScoped<IEventPublisher, MediatorPublisher>()
    .AddSingleton<IIdentityService, IdentityServiceMemory>()
    .AddMediatR(typeof(Program))

    // domain services
    .AddScoped<SectionDomainService>()
    .AddScoped<TokenSetDomainService>()

    // event handlers - HACK
    .AddScoped<IEventHandler<TokenValueSetEvent>, UpdateReleasesWhenTokenValueChanges>()

    // database
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddSingleton<ISectionRepository, SectionRepositoryMemory>()
    .AddSingleton<ITokenSetRepository, TokenSetRepositoryMemory>()
    .AddSingleton<IGlobalSchemaRepository, GlobalSchemaRepositoryMemory>()

    // queries
    .AddSingleton<ISectionQueries, SectionQueriesCoreRepository>()
    .AddSingleton<ITokenSetQueries, TokenSetQueriesCoreRepository>();

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

var tokenSetEntity = await tokenService.CreateTokenSetAsync("tokens1");
tokenSetEntity.SetValue("first", "Santa");
tokenSetEntity.SetValue("last", "Claus");

var tokenSet2Entity = await tokenService.CreateTokenSetAsync("tokens2", "tokens1");
tokenSet2Entity.SetValue("first", "SANTA!!!");

var modelValue = JsonDocument.Parse("{ \"firstName\": \"$$first$$\", \"lastName\": \"$$last$$\", \"age\": 44 }");
var composed = new TokenSetComposer(new[] {tokenSetEntity.ToTokenSet()}).Compose("tokens1");

var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await sectionService.CreateSectionAsync("name1", "path1");

var env1 = section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev");
section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev2");
var s = await GetSchema("1.0.0.json");
var schema1 = section1.AddSchema(await idService.GetId<SectionSchemaId>(), new SemanticVersion(1, 0, 0), s);
section1.AddSchema(await idService.GetId<SectionSchemaId>(), new SemanticVersion(2, 0, 0),
    await GetSchema("2.0.0.json"));

var release =
    await section1.CreateReleaseAsync(env1.Id, await idService.GetId<ReleaseId>(), composed, schema1.Id, modelValue);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now);

await section1.CreateReleaseAsync(env1.Id, await idService.GetId<ReleaseId>(), composed, schema1.Id, modelValue);
section1.SetDeployed(env1.Id, release.Id, await idService.GetId<DeploymentId>(), DateTime.Now);

await sectionService.CreateSectionAsync("name2", "path2");
await uow.SaveChangesAsync();

app.Run();

async Task<JsonSchema> GetSchema(string fileName)
{
    var f = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", fileName);
    var json = File.ReadAllText(f);
    //return await JsonSchema.FromJsonAsync(json);

    return await JsonSchema.FromJsonAsync(json, ".", s =>
    {
        var schemaResolver = new JsonSchemaResolver(s, new JsonSchemaGeneratorSettings());
        var referenceResolver = new Resolver(schemaResolver);

        var kafkaFile = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "__kafka-1.0.0.json");
        var kafkaSchema = JsonSchema.FromFileAsync(kafkaFile).Result;

        referenceResolver.AddDocumentReference("/ppm/kafka/1.0.0", kafkaSchema);
        return referenceResolver;
    });
}

public class Resolver : JsonReferenceResolver
{
    public Resolver(JsonSchemaAppender schemaAppender) : base(schemaAppender)
    {
    }

    public override Task<IJsonReference> ResolveFileReferenceAsync(string filePath,
        CancellationToken cancellationToken = new CancellationToken())
    {
        Console.WriteLine();
        return base.ResolveFileReferenceAsync(filePath, cancellationToken);
    }
}