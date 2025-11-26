/// <summary>
/// Service for generating procedural A-story scene templates
/// Creates infinite main story progression after authored tutorial completes
/// Works from ANY sequence - if tutorial is A1-A3, generates A4+
/// If tutorial expands to A1-A10, generates A11+
///
/// ARCHITECTURE PATTERN: Dynamic Template Package (HIGHLANDER-Compliant)
/// 1. Select archetype based on sequence/tier/context
/// 2. Generate SceneTemplateDTO with categorical properties
/// 3. Serialize to JSON package
/// 4. Load via PackageLoaderFacade (JSON → PackageLoader → Parser → Entity)
/// 5. Template added to GameWorld.SceneTemplates, available for spawning
///
/// INTEGRATION POINT: Called from SpawnFacade when A-story scene completes
/// Detection: scene.Category == MainStory && scene.MainStorySequence.HasValue
/// Trigger: Generate MainStorySequence + 1 template if not already exists
///
/// ARCHETYPE SELECTION STRATEGY:
/// - Rotate through archetype categories (investigation → social → confrontation → discovery → crisis)
/// - Avoid recent archetypes (5-scene anti-repetition window)
/// - Match tier escalation (personal → local → regional, grounded character-driven)
/// - Balance narrative variety and mechanical consistency
///
/// GUARANTEED PROGRESSION:
/// - All generated templates follow 4-choice pattern (stat/money/challenge/fallback)
/// - Every situation has guaranteed success path (fallback with no requirements)
/// - Final situation spawns next A-scene (ensures infinite forward progress)
/// </summary>
public class ProceduralAStoryService
{
    private readonly GameWorld _gameWorld;
    private readonly ContentGenerationFacade _contentFacade;
    private readonly PackageLoaderFacade _packageLoaderFacade;

    // Archetype categories for rotation
    // Archetype categories queried dynamically from catalog (HIGHLANDER - single source of truth)
    // Prevents drift between catalog and procedural selection
    // Categories retrieved at runtime via AStorySceneArchetypeCatalog.GetAvailableArchetypesByCategory()

    public ProceduralAStoryService(
        GameWorld gameWorld,
        ContentGenerationFacade contentFacade,
        PackageLoaderFacade packageLoaderFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _contentFacade = contentFacade ?? throw new ArgumentNullException(nameof(contentFacade));
        _packageLoaderFacade = packageLoaderFacade ?? throw new ArgumentNullException(nameof(packageLoaderFacade));
    }

    /// <summary>
    /// Generate next A-story template for given sequence
    /// Called when previous A-scene completes
    /// Returns generated template ID (for tracking)
    /// </summary>
    public async Task<string> GenerateNextATemplate(int sequence, AStoryContext context)
    {
        // 1. Select appropriate archetype
        string archetypeId = SelectArchetype(sequence, context);

        // 2. Calculate tier from sequence
        int tier = CalculateTier(sequence);

        // 3. Build SceneTemplateDTO
        SceneTemplateDTO dto = BuildSceneTemplateDTO(sequence, archetypeId, tier, context);

        // 4. Serialize to JSON package
        string packageJson = SerializeTemplatePackage(dto);
        string packageId = $"a_story_{sequence}_template";

        // 5. Write dynamic package file
        await _contentFacade.CreateDynamicPackageFile(packageJson, packageId);

        // 6. Load through HIGHLANDER pipeline (JSON → PackageLoader → Parser)
        // Result discarded - templates don't need post-load configuration
        _ = await _packageLoaderFacade.LoadDynamicPackage(packageJson, packageId);

        return dto.Id;
    }

    /// <summary>
    /// Select archetype for given sequence based on context
    /// Rotation strategy: investigation → social → confrontation → crisis (repeat)
    /// Anti-repetition: Avoid archetypes used in last 5 scenes
    /// Tier-appropriate: Match archetype complexity to tier
    /// Works from ANY sequence (flexible number of authored scenes)
    ///
    /// DYNAMIC CATALOG QUERY (HIGHLANDER - single source of truth):
    /// Queries AStorySceneArchetypeCatalog.GetArchetypesForCategory() at runtime
    /// Prevents drift between catalog implementation and procedural selection
    /// When new archetypes added to catalog, automatically available for selection
    /// </summary>
    private string SelectArchetype(int sequence, AStoryContext context)
    {
        // Determine archetype category by rotation cycle
        // Generic: works regardless of where procedural generation starts
        // Sequence 1: Investigation (0), Sequence 2: Social (1), etc.
        int cyclePosition = (sequence - 1) % 4;

        string categoryKey = cyclePosition switch
        {
            0 => "Investigation",
            1 => "Social",
            2 => "Confrontation",
            3 => "Crisis",
            _ => "Investigation"
        };

        // Query catalog for available archetypes (SINGLE SOURCE OF TRUTH)
        List<string> candidateArchetypes =
            AStorySceneArchetypeCatalog.GetArchetypesForCategory(categoryKey);

        // FAIL-FAST: Validate catalog returned archetypes (prevents division by zero)
        if (!candidateArchetypes.Any())
        {
            throw new InvalidOperationException(
                $"Cannot select archetype: Catalog returned no archetypes for category '{categoryKey}'. " +
                $"Sequence {sequence} maps to cycle position {cyclePosition}. " +
                $"Check AStorySceneArchetypeCatalog.GetArchetypesForCategory implementation.");
        }

        // Filter out recent archetypes (anti-repetition)
        List<string> availableArchetypes = candidateArchetypes
            .Where(a => !context.IsArchetypeRecent(a))
            .ToList();

        // If all recent (edge case), use any from category
        if (!availableArchetypes.Any())
        {
            availableArchetypes = candidateArchetypes;
        }

        // Select archetype using sequence-based rotation within category (deterministic but varied)
        // Modulo ensures we cycle through category archetypes instead of always picking first
        // Example: Investigation has 3 archetypes - Seq1→arch0, Seq5→arch1, Seq9→arch2, Seq13→arch0
        int selectionIndex = sequence % availableArchetypes.Count;
        string selectedArchetype = availableArchetypes[selectionIndex];

        return selectedArchetype;
    }

    /// <summary>
    /// Calculate tier from sequence number (grounded character-driven escalation)
    /// Works from ANY sequence - tier thresholds are absolute
    /// Tier 1: Sequence 1-30 (personal stakes - relationships, internal conflict)
    /// Tier 2: Sequence 31-50 (local stakes - community, village/town)
    /// Tier 3: Sequence 51+ (regional stakes - district/province, maximum scope)
    /// </summary>
    private int CalculateTier(int sequence)
    {
        if (sequence <= 30) return 1; // Personal
        if (sequence <= 50) return 2; // Local
        return 3; // Regional (infinite at maximum grounding)
    }

    /// <summary>
    /// Build SceneTemplateDTO for procedural A-story scene
    /// Uses categorical properties (no concrete entity IDs at generation time)
    /// PlacementFilter uses Generic relation for runtime resolution
    /// </summary>
    private SceneTemplateDTO BuildSceneTemplateDTO(
        int sequence,
        string archetypeId,
        int tier,
        AStoryContext context)
    {
        string sceneId = $"a_story_{sequence}";

        // Build categorical placement filter
        PlacementFilterDTO placementFilter = BuildPlacementFilter(sequence, tier, context);

        // Build spawn conditions (A-scenes spawn automatically via completion trigger)
        SpawnConditionsDTO spawnConditions = BuildSpawnConditions(sequence, context);

        SceneTemplateDTO dto = new SceneTemplateDTO
        {
            Id = sceneId,
            Archetype = "Linear", // A-story scenes are linear progression
            DisplayNameTemplate = $"The Path Deepens (A{sequence})", // AI will generate better title
            SceneArchetypeId = archetypeId, // Routes to AStorySceneArchetypeCatalog
            LocationActivationFilter = placementFilter, // A-story activates when player enters matching location
            SpawnConditions = spawnConditions,
            Tier = tier,
            Category = "MainStory",
            MainStorySequence = sequence,
            PresentationMode = "Modal", // A-story takes over screen (Sir Brante pattern)
            ProgressionMode = "Cascade", // Situations flow with momentum
            ExpirationDays = null, // A-story never expires
            IntroNarrativeTemplate = null, // AI generates from hints
            DependentLocations = null, // Catalogue generates if needed
            DependentItems = null // Catalogue generates if needed
        };

        return dto;
    }

    /// <summary>
    /// Build categorical placement filter for A-story scene
    /// Uses Generic relation for runtime resolution via GameWorld queries
    /// No concrete entity IDs - all categorical properties
    /// </summary>
    private PlacementFilterDTO BuildPlacementFilter(int sequence, int tier, AStoryContext context)
    {
        // Select region based on tier and anti-repetition
        Region selectedRegion = SelectRegion(tier, context);

        // Select NPC personality type for social archetypes
        List<string> personalityTypes = SelectPersonalityTypes(tier, context);

        PlacementFilterDTO filter = new PlacementFilterDTO
        {
            PlacementType = "Location", // A-story happens at locations

            // Location filters (categorical)
            // ZERO NULL TOLERANCE: selectedRegion guaranteed non-null by SelectRegion (returns first available or throws)
            RegionId = selectedRegion!.Name, // Categorical identifier: Region.Name (NOT entity instance ID)
            Capabilities = SelectLocationCapabilities(tier),
            LocationTags = new List<string> { "story_significant" },

            // NPC filters (categorical)
            PersonalityTypes = personalityTypes,
            MinBond = null, // A-story accessible regardless of relationships
            MaxBond = null,
            NpcTags = new List<string> { "order_connected" }
        };

        return filter;
    }

    /// <summary>
    /// Select region for A-story scene based on tier and anti-repetition
    /// Systematically rotates through available regions
    /// Returns Region object (HIGHLANDER: object references, not IDs)
    /// </summary>
    private Region SelectRegion(int tier, AStoryContext context)
    {
        // Get all regions in GameWorld
        List<Region> allRegions = _gameWorld.Regions;

        if (!allRegions.Any())
        {
            throw new InvalidOperationException(
                "Cannot select region: No regions defined in GameWorld. " +
                "Procedural A-story generation requires at least one region. " +
                "Check Content/Core/01_foundation.json for region definitions.");
        }

        // Filter by tier-appropriate regions
        List<Region> tierAppropriateRegions = allRegions
            .Where(r => r.Tier <= tier) // Only regions at or below current tier
            .ToList();

        if (!tierAppropriateRegions.Any())
        {
            tierAppropriateRegions = allRegions; // Fallback to any region
        }

        // Filter out recently used regions (anti-repetition)
        // HIGHLANDER: Pass Region object to IsRegionRecent, not ID
        List<Region> availableRegions = tierAppropriateRegions
            .Where(r => !context.IsRegionRecent(r))
            .ToList();

        if (!availableRegions.Any())
        {
            availableRegions = tierAppropriateRegions; // All recent, use any
        }

        // Select region using sequence-based rotation (deterministic but varied)
        // Modulo ensures we cycle through available regions: A4→region0, A5→region1, A6→region2, A7→region0...
        int selectionIndex = context.CurrentSequence % availableRegions.Count;
        Region selectedRegion = availableRegions[selectionIndex];

        // Return Region object (HIGHLANDER: object references, not string IDs)
        return selectedRegion;
    }

    /// <summary>
    /// Select personality types for NPC categorical filtering
    /// Varies based on tier and anti-repetition
    /// </summary>
    private List<string> SelectPersonalityTypes(int tier, AStoryContext context)
    {
        List<PersonalityType> allTypes = new List<PersonalityType>
    {
        PersonalityType.DEVOTED,
        PersonalityType.MERCANTILE,
        PersonalityType.PROUD,
        PersonalityType.CUNNING,
        PersonalityType.STEADFAST
    };

        // Filter out recent personality types (anti-repetition)
        List<PersonalityType> availableTypes = allTypes
            .Where(p => !context.IsPersonalityTypeRecent(p))
            .ToList();

        if (!availableTypes.Any())
        {
            availableTypes = allTypes; // All recent, use any
        }

        // Select 2-3 personality types for variety
        int countToSelect = Math.Min(2 + (tier - 1), availableTypes.Count);
        List<string> selectedTypes = availableTypes
            .Take(countToSelect)
            .Select(p => p.ToString())
            .ToList();

        return selectedTypes;
    }

    /// <summary>
    /// Select location capabilities for categorical filtering
    /// Tier-appropriate capabilities (higher tier = more complex locations)
    /// Returns string list for PlacementFilterDTO (parser will convert to enum)
    /// </summary>
    private List<string> SelectLocationCapabilities(int tier)
    {
        List<string> capabilities = new List<string> { "Indoor", "Urban" };

        // Add tier-appropriate capabilities
        if (tier >= 2)
        {
            capabilities.Add("Commercial");
        }

        if (tier >= 3)
        {
            capabilities.Add("Official");
        }

        if (tier >= 4)
        {
            capabilities.Add("Temple"); // Mysterious equivalent
        }

        return capabilities;
    }

    /// <summary>
    /// Build spawn conditions for A-story scene
    /// A-scenes spawn sequentially via ScenesToSpawn rewards
    /// No explicit conditions needed - progression managed by reward chain
    /// </summary>
    private SpawnConditionsDTO BuildSpawnConditions(int sequence, AStoryContext context)
    {
        // No spawn conditions - A-story progression managed by sequential spawning
        // Each A-scene spawns the next via ScenesToSpawn reward in final situation
        // Return null - procedural A-scenes spawn via reward system, not spawn conditions
        return null;
    }

    /// <summary>
    /// Serialize SceneTemplateDTO to JSON package format
    /// Package contains single scene template for HIGHLANDER pipeline
    /// </summary>
    private string SerializeTemplatePackage(SceneTemplateDTO dto)
    {
        // Create package wrapper (same format as authored JSON packages)
        ProceduralTemplatePackage package = new ProceduralTemplatePackage
        {
            PackageId = $"a_story_{dto.MainStorySequence}_template",
            Version = "1.0",
            GeneratedAt = DateTime.UtcNow.ToString("o"),
            SceneTemplates = new List<SceneTemplateDTO> { dto }
        };

        // Serialize with pretty formatting for debugging
        System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        string json = System.Text.Json.JsonSerializer.Serialize(package, options);

        return json;
    }

    /// <summary>
    /// Check if next A-scene template already exists in GameWorld
    /// Prevents duplicate generation
    /// </summary>
    public bool NextTemplateExists(int nextSequence)
    {
        string nextTemplateId = $"a_story_{nextSequence}";

        bool exists = _gameWorld.SceneTemplates.Any(t =>
            t.Category == StoryCategory.MainStory &&
            t.MainStorySequence == nextSequence);

        return exists;
    }

    /// <summary>
    /// Get or initialize AStoryContext from GameWorld player state
    /// Context tracks progression for intelligent generation decisions
    /// </summary>
    public AStoryContext GetOrInitializeContext(Player player)
    {
        // Check if player has completed any A-story scenes
        List<Scene> completedAScenes = _gameWorld.Scenes
            .Where(s => s.Category == StoryCategory.MainStory &&
                        s.MainStorySequence.HasValue &&
                        s.State == SceneState.Completed)
            .OrderBy(s => s.MainStorySequence)
            .ToList();

        if (!completedAScenes.Any())
        {
            // No A-story progression yet, initialize empty context
            return AStoryContext.InitializeForProceduralGeneration();
        }

        // Reconstruct context from completed scenes
        Scene lastCompleted = completedAScenes.Last();
        int lastSequence = lastCompleted.MainStorySequence.Value;

        AStoryContext context = new AStoryContext
        {
            CurrentSequence = lastSequence + 1,
            LastCompletedSequence = lastSequence,
            CompletedScenes = completedAScenes.ToList(),  // HIGHLANDER: Store scene objects, not IDs
            RecentArchetypeIds = new List<string>(),
            RecentRegions = new List<Region>(),
            RecentPersonalityTypes = new List<PersonalityType>(),
            UnlockedRegionIds = new List<string>(),
            EncounteredOrderMemberIds = new List<string>(),
            CollectedArtifactIds = new List<string>(),
            UncoveredRevelationIds = new List<string>(),
            CurrentPursuitGoal = "Discover the fate of the scattered Order"
        };

        // Reconstruct anti-repetition windows from recent scenes
        List<Scene> recentScenes = completedAScenes.TakeLast(5).ToList();
        foreach (Scene scene in recentScenes)
        {
            // Validate and extract archetype from template
            if (scene.Template == null)
            {
                throw new InvalidOperationException(
                    $"A-story scene (MainStorySequence={scene.MainStorySequence}) has null Template. " +
                    $"All scenes must have valid Template reference.");
            }
            if (string.IsNullOrEmpty(scene.Template.SceneArchetypeId))
            {
                throw new InvalidOperationException(
                    $"A-story scene template (MainStorySequence={scene.MainStorySequence}) has null or empty SceneArchetypeId. " +
                    $"All A-story scenes must have archetype.");
            }
            context.RecentArchetypeIds.Add(scene.Template.SceneArchetypeId);

            // Validate and extract region from LAST COMPLETED situation
            if (!scene.Situations.Any())
            {
                throw new InvalidOperationException(
                    $"A-story scene (MainStorySequence={scene.MainStorySequence}) has no situations. " +
                    $"All A-story scenes must have at least one situation.");
            }
            Situation lastSituation = scene.Situations.Last();
            if (lastSituation.Location == null)
            {
                throw new InvalidOperationException(
                    $"A-story situation '{lastSituation.Name}' has null Location. " +
                    $"All A-story situations must have Location for context tracking.");
            }
            Location situationLocation = lastSituation.Location;
            if (situationLocation.Venue == null || situationLocation.Venue.District == null || situationLocation.Venue.District.Region == null)
            {
                throw new InvalidOperationException(
                    $"A-story location '{situationLocation.Name}' has incomplete spatial hierarchy. " +
                    $"Venue={situationLocation.Venue?.Name ?? "null"}, " +
                    $"District={situationLocation.Venue?.District?.Name ?? "null"}, " +
                    $"Region={situationLocation.Venue?.District?.Region?.Name ?? "null"}. " +
                    $"All A-story locations must have complete Venue→District→Region chain.");
            }
            Region region = situationLocation.Venue.District.Region;
            if (!context.RecentRegions.Contains(region))
            {
                context.RecentRegions.Add(region);
            }

            // Validate and extract NPC personality from last situation
            if (lastSituation.Npc == null)
            {
                throw new InvalidOperationException(
                    $"A-story situation '{lastSituation.Name}' has null NPC. " +
                    $"All A-story situations require NPC interaction (location-only situations not allowed in A-story progression).");
            }
            NPC situationNpc = lastSituation.Npc;
            if (!context.RecentPersonalityTypes.Contains(situationNpc.PersonalityType))
            {
                context.RecentPersonalityTypes.Add(situationNpc.PersonalityType);
            }
        }

        return context;
    }
}

/// <summary>
/// Strongly-typed package wrapper for procedurally-generated scene templates
/// Replaces anonymous type - HIGHLANDER compliance
/// </summary>
public class ProceduralTemplatePackage
{
    public string PackageId { get; set; }
    public string Version { get; set; }
    public string GeneratedAt { get; set; }
    public List<SceneTemplateDTO> SceneTemplates { get; set; }
}
