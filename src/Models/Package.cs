using System;

/// <summary>
/// Main package container for game content
/// </summary>
public class Package
{
    /// <summary>
    /// Unique identifier for this package
    /// </summary>
    public string PackageId { get; set; }

    /// <summary>
    /// Metadata about the package
    /// </summary>
    public PackageMetadata Metadata { get; set; }

    /// <summary>
    /// The actual game content
    /// </summary>
    public PackageContent Content { get; set; }

    /// <summary>
    /// Optional starting conditions for new games - reuses existing PlayerInitialConfig
    /// </summary>
    public PackageStartingConditions StartingConditions { get; set; }
}