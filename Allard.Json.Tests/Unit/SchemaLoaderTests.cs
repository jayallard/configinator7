using System;
using System.IO;
using System.Threading.Tasks;
using Allard.Json.NJsonSchema;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Allard.Json.Tests.Unit;

public class SchemaLoaderTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SchemaLoaderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Blah()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", "test1.json");
        var text = await File.ReadAllTextAsync(file);
        var json = JObject.Parse(text);

        var loader = new SchemaLoader(new JsonSchemaResolverFile(id =>
        {
            var parts = id.Id.Split("/");
            var fileName = Path.Combine(Directory.GetCurrentDirectory(), "Schemas", $"__{parts[0]}-{parts[1]}.json");
            return new FileInfo(fileName);
        }));

        var schema = await loader.LoadAsync(json);
        _testOutputHelper.WriteLine(schema.ToJson());
    }
}