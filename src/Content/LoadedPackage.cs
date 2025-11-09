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

public LoadedPackage()
{
    LoadedAt = DateTime.Now;
}
}