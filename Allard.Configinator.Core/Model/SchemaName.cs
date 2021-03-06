using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public record SchemaName
{
    public SchemaName(string name)
    {
        var slash = name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
        if (slash < 1)
            throw new InvalidOperationException("Invalid schema name: " + name + ". Naming convention is name/Version");

        Name = name[..slash];
        Version = SemanticVersion.Parse(name[(slash + 1)..]);
        FullName = name;
    }

    public SemanticVersion Version { get; }
    public string Name { get; }

    public string FullName { get; }

    public static SchemaName Parse(string name)
    {
        return new SchemaName(name);
    }
}