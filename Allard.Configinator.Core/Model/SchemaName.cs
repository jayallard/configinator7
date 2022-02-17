using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public class SchemaName
{
    public static SchemaName Parse(string name) => new SchemaName(name);

    public SemanticVersion Version { get; }
    public string Name { get; }

    public SchemaName(string name)
    {
        var slash = name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
        if (slash < 2)
        {
            throw new InvalidOperationException("Invalid schema name: " + name + ". Naming convention is name/Version");
        }

        Name = name[..slash];
        Version = SemanticVersion.Parse(name[(slash + 1)..]);
    }
}