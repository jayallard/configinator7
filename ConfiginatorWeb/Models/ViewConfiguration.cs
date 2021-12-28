using System.ComponentModel.DataAnnotations;
using NuGet.Versioning;

namespace ConfiginatorWeb.Models;

public class ViewConfiguration
{
    [Key]
    public string Name { get; set; }
    
    [Required]
    public string Path { get; set; }
    
    public List<ViewSchema> Schemas { get; set; }
    
    public List<ViewHabitat> Habitats { get; set; }
}

public class ViewSchema
{
    public SemanticVersion Version { get; set; }
    public string Text { get; set; }
}

public class ViewHabitat
{
    public string Name { get; set; }
    
    public List<ViewRelease> Releases { get; set; }
    
    
}

public class ViewRelease
{
    public SemanticVersion Version { get; set; }
}