
/// <summary>
/// FACADE: Clean boundary for dynamic package loading
///
/// PURPOSE:
/// - Isolates game code from PackageLoader infrastructure
/// - Provides clean interface for loading dynamically-generated content
/// - Wraps package parsing and entity instantiation
///
/// USAGE:
/// - Called from: GameFacade during scene spawning pipeline
/// - Never calls other facades
/// - Only knows domain entities and PackageLoader
///
/// ISOLATION:
/// - Never imports SceneGenerationFacade, SceneInstanceFacade, or ContentGenerationFacade
/// - Only imported by GameFacade (orchestrator)
/// - Pure infrastructure boundary
/// </summary>
public class PackageLoaderFacade
{
    private readonly PackageLoader _packageLoader;

    public PackageLoaderFacade(PackageLoader packageLoader)
    {
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
    }

    /// <summary>
    /// Load dynamically-generated package from JSON string
    /// Creates Location/Item/NPC/Scene entities in GameWorld
    /// Returns PackageLoadResult with direct object references to all created entities
    /// HIGHLANDER: Callers use object references directly, not string-based lookups
    /// </summary>
    public async Task<PackageLoadResult> LoadDynamicPackage(string packageJson, string packageId)
    {
        return await _packageLoader.LoadDynamicPackageFromJson(packageJson, packageId);
    }

    /// <summary>
    /// Load single location from JSON file path
    /// Reads file, parses JSON, creates Location entity in GameWorld
    /// Returns created location ID
    /// </summary>
    public async Task<string> LoadDynamicLocation(string jsonFilePath)
    {
        string packageJson = File.ReadAllText(jsonFilePath);
        string packageId = Path.GetFileNameWithoutExtension(jsonFilePath);

        await _packageLoader.LoadDynamicPackageFromJson(packageJson, packageId);

        return packageId;
    }

    /// <summary>
    /// Create a single location directly from DependentLocationSpec.
    /// NO JSON serialization - creates Location in-memory and returns direct reference.
    /// HIGHLANDER: Direct creation path for situation binding, no matching by name/ID.
    /// </summary>
    public Location CreateSingleLocation(DependentLocationSpec spec, Venue contextVenue)
    {
        return _packageLoader.CreateSingleLocation(spec, contextVenue);
    }

    /// <summary>
    /// Create a single NPC directly from DependentNpcSpec.
    /// NO JSON serialization - creates NPC in-memory and returns direct reference.
    /// HIGHLANDER: Direct creation path for situation binding, no matching by name/ID.
    /// </summary>
    public NPC CreateSingleNpc(DependentNpcSpec spec, Location contextLocation)
    {
        return _packageLoader.CreateSingleNpc(spec, contextLocation);
    }
}
