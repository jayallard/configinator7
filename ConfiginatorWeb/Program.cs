using Configinator7.Core.Model;
using ConfiginatorWeb.Projections;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NuGet.Versioning;

var builder = WebApplication.CreateBuilder(args);

// working in memory... set it up for demo/testing
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

agg.AddEnvironment("abc", "dev");
agg.AddEnvironment("abc", "dev-jay");
agg.AddEnvironment("abc", "staging");
agg.AddEnvironment("abc", "production");

await agg.CreateReleaseAsync("abc", "dev", new SemanticVersion(2, 0, 0),
    (JObject) JToken.FromObject(new {firstName = "Like", lastName = "Skywalker"}));
await agg.CreateReleaseAsync("abc", "dev", new SemanticVersion(1, 0, 1), JObject.Parse("{}"));
await agg.CreateReleaseAsync("abc", "dev", new SemanticVersion(1, 0, 0), JObject.Parse("{}"));

agg.Deploy("abc", "dev", 1);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(agg);
builder.Services.AddTransient<IConfigurationProjections, ConfigurationProjectionsFromAggregate>();
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

app.Run();