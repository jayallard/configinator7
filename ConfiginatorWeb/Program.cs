using System.Text.Json;
using Allard.Configinator.Core.DomainServices;
using Allard.Configinator.Core.Model;
using Allard.Configinator.Core.Repositories;
using Allard.Configinator.Infrastructure;
using Allard.DomainDrivenDesign;
using ConfiginatorWeb.Queries;
using MediatR;
using NJsonSchema;
using NuGet.Versioning;

var builder = WebApplication.CreateBuilder(args);

// working in memory... set it up for demo/testing
/*
var agg = new SuperAggregate();
agg.CreateSection("abc", null, "/a/b/c", null);
agg.CreateSection("xyz", null, "/x/y/z", null);

var schema1 = File.ReadAllText((Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "2.0.0.json")));
agg.AddSchema("abc",
    new ConfigurationSchema(new SemanticVersion(1, 0, 0), await JsonSchema.FromJsonAsync("{ _version: \"1.0.0\" }")));
agg.AddSchema("abc",
    new ConfigurationSchema(new SemanticVersion(1, 0, 1), await JsonSchema.FromJsonAsync("{ _version: \"1.0.1\" }")));
agg.AddSchema("abc",
    new ConfigurationSchema(new SemanticVersion(1, 0, 2), await JsonSchema.FromJsonAsync("{ _version: \"1.0.2\" }")));
agg.AddSchema("abc", new ConfigurationSchema(new SemanticVersion(2, 0, 0), await JsonSchema.FromJsonAsync(schema1)));

agg.AddTokenSet("Tokens0", new()
{
    {"ShoeSize", 8.322},
    {"SomethingCool", "abc"},
    {"t1", 17}
});
agg.AddTokenSet("Tokens1", new()
{
    {"FavoriteColor", "Clear"},
    {"ShoeSize", 7.7998732}
}, "Tokens0");
agg.AddTokenSet("Tokens2", new Dictionary<string, JToken>
{
    {"FavoriteColor", "Red"},
    {"t1", 99},
    {"t2", "string"},
    {"t3", JToken.Parse("{ \"hello\": \"galaxy\", \"FavoriteColor\": \"$$FavoriteColor$$\", \"AwesomeNumber\": \"$$t1$$\", \"FFF\": \"$$firstname$$\", \"LLL\": \"$$LastName$$\" }")},
    {"FirstName", "Han"},
    {"LastName", "Solo"}
}, "Tokens1");

agg.AddEnvironment("abc", "dev");
agg.AddEnvironment("abc", "dev-jay");
agg.AddEnvironment("abc", "staging");
agg.AddEnvironment("abc", "production");

await agg.CreateReleaseAsync("abc", "dev", null, new SemanticVersion(1, 0, 1), JObject.Parse("{}"));
await agg.CreateReleaseAsync("abc", "dev", null, new SemanticVersion(1, 0, 0), JObject.Parse("{}"));
await agg.CreateReleaseAsync("abc", "dev", "Tokens2", new SemanticVersion(2, 0, 0),
    (JObject) JToken.FromObject(new {firstName = "$$FirstName$$", lastName = "$$LastName$$", whatever="$$t3$$" }));

agg.Deploy("abc", "dev", 1);
*/

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services
    .AddSingleton<IEventPublisher, MediatorPublisher>()
    .AddSingleton<IDatabase, DatabaseMemory>()
    .AddScoped<ISectionRepository, SectionRepositoryMemory>()
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddScoped<ISectionQueries, SectionQueriesDatabase>()
    .AddScoped<IIdentityService, IdentityServiceMemory>()
    .AddScoped<SectionDomainService>()
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddMediatR(typeof(Program));

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


//var section1 = new SectionEntity(new SectionId(1), "abc", "/abc");
//var section2 = new SectionEntity(new SectionId(2), "xyz", "/xyz");


using var scope = app.Services.CreateScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
var domainService = scope.ServiceProvider.GetRequiredService<SectionDomainService>();
var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await domainService.CreateSectionAsync("name1", "path1");

// todo: shouldn't pass in the id
var env1 = section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev");
section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev2");
var schema1 = section1.AddSchema(await idService.GetId<SchemaId>(), new SemanticVersion(1, 0, 0),
    await JsonSchema.FromJsonAsync("{}"));
section1.AddSchema(await idService.GetId<SchemaId>(), new SemanticVersion(2, 0, 0),
    await JsonSchema.FromJsonAsync("{}"));


var release = await env1.CreateReleaseAsync(await idService.GetId<ReleaseId>(), null, schema1.Id, JsonDocument.Parse("{}"));
release.SetDeployed(await idService.GetId<DeploymentHistoryId>(), DateTime.Now);

var section2 = await domainService.CreateSectionAsync("name2", "path2");
await uow.SaveChangesAsync();

app.Run();