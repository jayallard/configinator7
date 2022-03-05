namespace Allard.Configinator.Core.DomainServices;

// TODO: minimal implementation; rules are hardcoded elsewhere. mocking it up.

/// <summary>
/// </summary>
public class EnvironmentRules
{
    /// <summary>
    ///     Gets or sets the environment type / environment configuration.
    /// </summary>
    public List<EnvironmentType> EnvironmentTypes { get; set; }
}

public class EnvironmentType
{
    /// <summary>
    ///     Gets or sets the name of th environment type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the names of the environments for
    ///     this environment type.
    /// </summary>
    public string[] AllowedEnvironments { get; set; }
}