using NuGet.Versioning;

namespace Allard.Configinator.Core.Model;

public record SchemaName
{
    public SchemaName(string fullName)
    {
        var slash = fullName.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
        if (slash < 1)
            throw new InvalidOperationException("Invalid schema name: " + fullName + ". Naming convention is name/Version");

        Name = fullName[..slash];
        Version = SemanticVersion.Parse(fullName[(slash + 1)..]);
        FullName = fullName;
    }

    public SemanticVersion Version { get; }
    public string Name { get; }

    public string FullName { get; }

    public static SchemaName Parse(string name)
    {
        return new SchemaName(name);
    }
}