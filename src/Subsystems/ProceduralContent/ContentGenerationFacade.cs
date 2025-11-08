using System.Text.Json;
using Wayfarer.Content;

/// <summary>
/// FACADE: File I/O operations for dynamic content generation
///
/// PURPOSE:
/// - Transform LocationCreationSpec → JSON files
/// - Manage dynamic content directory structure
/// - Provide file manifest for debugging
///
/// BOUNDARIES:
/// - INPUT: Strongly-typed specs (LocationCreationSpec)
/// - OUTPUT: File paths and manifests
/// - NO parsing, NO GameWorld manipulation, NO orchestration
///
/// DEPENDENCIES: NONE (pure file operations)
/// </summary>
public class ContentGenerationFacade
{
    private const string DynamicContentDirectory = "Content/Dynamic";

    /// <summary>
    /// Create JSON file for dynamically generated package
    /// Writes to Content/Dynamic/{packageId}.json
    /// </summary>
    public DynamicFileResult CreateDynamicPackageFile(string packageJson, string packageId)
    {
        Directory.CreateDirectory(DynamicContentDirectory);

        string filePath = Path.Combine(DynamicContentDirectory, $"{packageId}.json");

        File.WriteAllText(filePath, packageJson);

        return new DynamicFileResult
        {
            FilePath = filePath,
            PackageId = packageId,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Remove dynamic content file
    /// </summary>
    public void RemoveDynamicLocation(string packageId)
    {
        if (string.IsNullOrEmpty(packageId)) throw new ArgumentException("PackageId cannot be empty", nameof(packageId));

        string filePath = Path.Combine(DynamicContentDirectory, $"{packageId}.json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Get manifest of all dynamic content files
    /// Used for debugging and cleanup
    /// </summary>
    public DynamicContentManifest GetDynamicContentManifest()
    {
        DynamicContentManifest manifest = new DynamicContentManifest();

        if (!Directory.Exists(DynamicContentDirectory))
        {
            return manifest;
        }

        List<string> jsonFiles = Directory.GetFiles(DynamicContentDirectory, "*.json").ToList();

        foreach (string filePath in jsonFiles)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            manifest.Files.Add(new DynamicFileInfo
            {
                FilePath = filePath,
                FileName = fileInfo.Name,
                PackageId = Path.GetFileNameWithoutExtension(filePath),
                SizeBytes = fileInfo.Length,
                CreatedUtc = fileInfo.CreationTimeUtc,
                ModifiedUtc = fileInfo.LastWriteTimeUtc
            });
        }

        return manifest;
    }

}

/// <summary>
/// Result of file creation operation
/// </summary>
public class DynamicFileResult
{
    public string FilePath { get; set; } = "";
    public string PackageId { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// File information for debugging
/// </summary>
public class DynamicFileInfo
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string PackageId { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }
}

/// <summary>
/// Manifest of all dynamic content files
/// </summary>
public class DynamicContentManifest
{
    public List<DynamicFileInfo> Files { get; set; } = new();
    public int TotalFiles => Files.Count;
    public long TotalSizeBytes => Files.Sum(f => f.SizeBytes);
}
