namespace Allard.Configinator.Core.DomainServices;

public class EnvironmentRules
{
    public List<EnvironmentType> EnvironmentTypes { get; set; }
}

public class EnvironmentType
{
    public string EnvironmentTypeName { get; set; }
    public string[] AllowedEnvironments { get; set; }
}