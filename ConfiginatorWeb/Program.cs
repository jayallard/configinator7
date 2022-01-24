using System.Text.Json;
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
using NuGet.Versioning;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services
    .AddSingleton<IEventPublisher, MediatorPublisher>()
    .AddScoped<IIdentityService, IdentityServiceMemory>()

    // domain services
    .AddScoped<SectionDomainService>()
    .AddScoped<TokenSetDomainService>()
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddSingleton<ISectionRepository, SectionRepositoryMemory>()
    .AddSingleton<ITokenSetRepository, TokenSetRepositoryMemory>()
    .AddScoped<IUnitOfWork, UnitOfWorkMemory>()
    .AddScoped<ISectionQueries, SectionQueriesDatabase>()
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

var s1File = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "test2.json");
var s1 = await JsonSchema.FromFileAsync(s1File);


using var scope = app.Services.CreateScope();
var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

var sectionService = scope.ServiceProvider.GetRequiredService<SectionDomainService>();
var tokenService = scope.ServiceProvider.GetRequiredService<TokenSetDomainService>();

var tokenSetEntity = await tokenService.CreateTokenSetAsync("tokens1");
tokenSetEntity.SetValue("first", "Santa");
tokenSetEntity.SetValue("last", "Claus");

var modelValue = JsonDocument.Parse("{ \"firstName\": \"$$first$$\", \"lastName\": \"$$last$$\", \"age\": 44 }");
var composed = new TokenSetComposer(new[] {tokenSetEntity.ToTokenSet()}).Compose("tokens1");

var idService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
var section1 = await sectionService.CreateSectionAsync("name1", "path1");

var env1 = section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev");
section1.AddEnvironment(await idService.GetId<EnvironmentId>(), "dev2");
var schema1 = section1.AddSchema(await idService.GetId<SchemaId>(), new SemanticVersion(1, 0, 0), s1);
section1.AddSchema(await idService.GetId<SchemaId>(), new SemanticVersion(2, 0, 0),
    await JsonSchema.FromJsonAsync("{}"));

var release = await env1.CreateReleaseAsync(await idService.GetId<ReleaseId>(), composed, schema1.Id, modelValue);
release.SetDeployed(await idService.GetId<DeploymentHistoryId>(), DateTime.Now);

var section2 = await sectionService.CreateSectionAsync("name2", "path2");
await uow.SaveChangesAsync();

app.Run();