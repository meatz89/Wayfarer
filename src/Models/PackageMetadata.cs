using System;

/// <summary>
/// Metadata about a content package
/// </summary>
public class PackageMetadata
{
    /// <summary>
    /// Human-readable name of the package
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// When this package was created
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Description of the package contents
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Who created this package (human or AI)
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Version of the package format
    /// </summary>
    public string Version { get; set; }
}