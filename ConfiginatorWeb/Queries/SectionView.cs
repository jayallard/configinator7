using NuGet.Versioning;

namespace ConfiginatorWeb.Queries;

public class SectionView
{
    public string SectionName { get; set; }
    
    public string Path { get; set; }
    
    public List<SectionSchemaView> Schemas { get; set; }
    
    public List<SectionEnvironmentView> Environments { get; set; }
}

public class SectionSchemaView
{
    public SemanticVersion Version { get; set; }
    public string Schema { get; set; }
}

public class SectionEnvironmentView
{
    public string EnvironmentName { get; set; }
    
    public List<SectionReleaseView> Releases { get; set; }
}

public class SectionReleaseView
{
    public long ReleaseId { get; set; }
    public SemanticVersion SchemaVersion { get; set; }
    
    public DateTime CreateDate { get; set; }
    
    public bool IsDeployed { get; set; }
    
    public bool IsOutOfDate { get; set; }
    
    public string TokenSetName { get; set; }
}