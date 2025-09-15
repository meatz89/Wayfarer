using System;

/// <summary>
/// Tracks packages that have been loaded into the game world.
/// Used to prevent duplicate loading and track package history.
/// </summary>
public class LoadedPackage
{
    /// <summary>
    /// Unique identifier of the loaded package
    /// </summary>
    public string PackageId { get; set; }

    /// <summary>
    /// Path to the package file (if loaded from file)
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// When this package was loaded
    /// </summary>
    public DateTime LoadedAt { get; set; }

    /// <summary>
    /// Order in which this package was applied (for dependency resolution)
    /// </summary>
    public int LoadOrder { get; set; }

    /// <summary>
    /// Whether this was loaded at startup or dynamically at runtime
    /// </summary>
    public bool IsDynamicContent { get; set; }

    /// <summary>
    /// Package metadata for display/debugging
    /// </summary>
    public PackageMetadata Metadata { get; set; }

    /// <summary>
    /// Count of entities loaded from this package
    /// </summary>
    public EntityCounts EntityCounts { get; set; }

    public LoadedPackage()
    {
        LoadedAt = DateTime.Now;
        EntityCounts = new EntityCounts();
    }
}

/// <summary>
/// Tracks counts of different entity types loaded from a package
/// </summary>
public class EntityCounts
{
    public int Regions { get; set; }
    public int Districts { get; set; }
    public int Locations { get; set; }
    public int Spots { get; set; }
    public int NPCs { get; set; }
    public int Cards { get; set; }
    public int Routes { get; set; }
    public int Items { get; set; }
    public int LetterTemplates { get; set; }
    public int Exchanges { get; set; }
    public int Events { get; set; }
    public int Obligations { get; set; }

    public int TotalEntities =>
        Regions + Districts + Locations + Spots + NPCs +
        Cards + Routes + Items + LetterTemplates +
        Exchanges + Events + Obligations;
}