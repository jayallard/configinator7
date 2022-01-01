using System.ComponentModel.DataAnnotations;
using NuGet.Versioning;

namespace ConfiginatorWeb.Models.Configuration;

public class ViewConfiguration
{
    [Key]
    public string Name { get; set; }
    
    [Required]
    public string Path { get; set; }
    
    public List<ViewSchema> Schemas { get; set; }
    
    public List<ViewEnvironment> Environments { get; set; }
}

public class ViewSchema
{
    public SemanticVersion Version { get; set; }
    public string Text { get; set; }
}

public class ViewEnvironment
{
    public string Name { get; set; }
    
    public List<ViewRelease> Releases { get; set; }
    
    
}

public class ViewRelease
{
    public long ReleaseId { get; set; }
    public SemanticVersion Version { get; set; }
    
    public DateTime CreateDate { get; set; }
    
    public bool IsDeployed { get; set; }
}