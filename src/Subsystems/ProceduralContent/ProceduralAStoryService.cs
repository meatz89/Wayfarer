/// <summary>
/// Service for generating procedural A-story scene templates
/// Creates infinite main story progression (A11, A12, A13... infinity)
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
    private static readonly List<string> InvestigationArchetypes = new List<string>
{
    "investigate_location",
    "gather_testimony",
    "uncover_conspiracy",
    "discover_artifact"
};

    private static readonly List<string> SocialArchetypes = new List<string>
{
    "meet_order_member",
    "gain_trust",
    "social_infiltration"
};

    private static readonly List<string> ConfrontationArchetypes = new List<string>
{
    "seek_audience",
    "confront_antagonist",
    "challenge_authority",
    "expose_corruption"
};

    private static readonly List<string> CrisisArchetypes = new List<string>
{
    "urgent_decision",
    "moral_crossroads",
    "sacrifice_choice",
    "reveal_truth"
};

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
        Console.WriteLine($"[ProceduralAStory] Generating A{sequence} template...");

        // 1. Select appropriate archetype
        string archetypeId = SelectArchetype(sequence, context);
        Console.WriteLine($"[ProceduralAStory] Selected archetype: {archetypeId}");

        // 2. Calculate tier from sequence
        int tier = CalculateTier(sequence);
        Console.WriteLine($"[ProceduralAStory] Calculated tier: {tier}");

        // 3. Build SceneTemplateDTO
        SceneTemplateDTO dto = BuildSceneTemplateDTO(sequence, archetypeId, tier, context);
        Console.WriteLine($"[ProceduralAStory] Built DTO for scene: {dto.Id}");

        // 4. Serialize to JSON package
        string packageJson = SerializeTemplatePackage(dto);
        string packageId = $"a_story_{sequence}_template";
        Console.WriteLine($"[ProceduralAStory] Serialized package: {packageId}");

        // 5. Write dynamic package file
        await _contentFacade.CreateDynamicPackageFile(packageJson, packageId);
        Console.WriteLine($"[ProceduralAStory] Wrote dynamic package file");

        // 6. Load through HIGHLANDER pipeline (JSON → PackageLoader → Parser)
        await _packageLoaderFacade.LoadDynamicPackage(packageJson, packageId);
        Console.WriteLine($"[ProceduralAStory] Loaded package through HIGHLANDER pipeline");

        // Template now in GameWorld.SceneTemplates, ready for spawning
        Console.WriteLine($"[ProceduralAStory] A{sequence} template generation complete: {dto.Id}");

        return dto.Id;
    }

    /// <summary>
    /// Select archetype for given sequence based on context
    /// Rotation strategy: investigation → social → confrontation → crisis (repeat)
    /// Anti-repetition: Avoid archetypes used in last 5 scenes
    /// Tier-appropriate: Match archetype complexity to tier
    /// </summary>
    private string SelectArchetype(int sequence, AStoryContext context)
    {
        // Determine archetype category by rotation cycle
        int cyclePosition = (sequence - 11) % 4;

        List<string> candidateArchetypes = cyclePosition switch
        {
            0 => InvestigationArchetypes,
            1 => SocialArchetypes,
            2 => ConfrontationArchetypes,
            3 => CrisisArchetypes,
            _ => InvestigationArchetypes
        };

        // Filter out recent archetypes (anti-repetition)
        List<string> availableArchetypes = candidateArchetypes
            .Where(a => !context.IsArchetypeRecent(a))
            .ToList();

        // If all recent (edge case), use any from category
        if (!availableArchetypes.Any())
        {
            availableArchetypes = candidateArchetypes;
        }

        // Select first available (deterministic for given sequence)
        // Could add randomization here, but deterministic ensures reproducibility
        string selectedArchetype = availableArchetypes.First();

        return selectedArchetype;
    }

    /// <summary>
    /// Calculate tier from sequence number (grounded character-driven escalation)
    /// Tier 1: A11-A30 (personal stakes - relationships, internal conflict)
    /// Tier 2: A31-A50 (local stakes - community, village/town)
    /// Tier 3: A51+ (regional stakes - district/province, maximum scope)
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
            PlacementFilter = placementFilter,
            SpawnConditions = spawnConditions,
            Tier = tier,
            Category = "MainStory",
            MainStorySequence = sequence,
            PresentationMode = "Modal", // A-story takes over screen (Sir Brante pattern)
            ProgressionMode = "Cascade", // Situations flow with momentum
            IsStarter = false,
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
        string regionId = SelectRegion(tier, context);

        // Select NPC personality type for social archetypes
        List<string> personalityTypes = SelectPersonalityTypes(tier, context);

        PlacementFilterDTO filter = new PlacementFilterDTO
        {
            PlacementType = "Location", // A-story happens at locations

            // Location filters (categorical)
            RegionId = regionId, // Specific region for tier-appropriate content
            LocationProperties = SelectLocationProperties(tier),
            LocationTags = new List<string> { "story_significant" },

            // NPC filters (categorical)
            PersonalityTypes = personalityTypes,
            MinBond = null, // A-story accessible regardless of relationships
            MaxBond = null,
            NpcTags = new List<string> { "order_connected" },

            // No concrete IDs (Generic placement resolves at spawn time)
            LocationId = null,
            NpcId = null
        };

        return filter;
    }

    /// <summary>
    /// Select region for A-story scene based on tier and anti-repetition
    /// Systematically rotates through available regions
    /// </summary>
    private string SelectRegion(int tier, AStoryContext context)
    {
        // Get all regions in GameWorld
        List<Region> allRegions = _gameWorld.Regions;

        if (!allRegions.Any())
        {
            return null; // No regions defined (fallback to any location)
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
        List<Region> availableRegions = tierAppropriateRegions
            .Where(r => !context.IsRegionRecent(r.Id))
            .ToList();

        if (!availableRegions.Any())
        {
            availableRegions = tierAppropriateRegions; // All recent, use any
        }

        // Select first available (deterministic)
        return availableRegions.First().Id;
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
    /// Select location properties for categorical filtering
    /// Tier-appropriate properties (higher tier = more complex locations)
    /// </summary>
    private List<string> SelectLocationProperties(int tier)
    {
        List<string> properties = new List<string> { "indoor", "urban" };

        // Add tier-appropriate properties
        if (tier >= 2)
        {
            properties.Add("commercial");
        }

        if (tier >= 3)
        {
            properties.Add("political");
        }

        if (tier >= 4)
        {
            properties.Add("mysterious");
        }

        return properties;
    }

    /// <summary>
    /// Build spawn conditions for A-story scene
    /// A-scenes spawn automatically when previous A-scene completes
    /// Spawn conditions verify previous scene completion
    /// </summary>
    private SpawnConditionsDTO BuildSpawnConditions(int sequence, AStoryContext context)
    {
        // Previous A-scene must be completed
        string previousSceneId = $"a_story_{sequence - 1}";

        SpawnConditionsDTO conditions = new SpawnConditionsDTO
        {
            CombinationLogic = "All", // All conditions must pass

            // Player state conditions
            PlayerState = new PlayerStateConditionsDTO
            {
                CompletedScenes = new List<string> { previousSceneId }, // Previous A-scene completed
                MinStats = new Dictionary<string, int>(), // No stat gates (accessible to all)
                RequiredItems = new List<string>() // No item requirements
            },

            // No world state conditions (time/weather irrelevant for A-story)
            WorldState = null,

            // No entity state conditions (spawn independent of entity states)
            EntityState = null
        };

        return conditions;
    }

    /// <summary>
    /// Serialize SceneTemplateDTO to JSON package format
    /// Package contains single scene template for HIGHLANDER pipeline
    /// </summary>
    private string SerializeTemplatePackage(SceneTemplateDTO dto)
    {
        // Create package wrapper (same format as authored JSON packages)
        var package = new
        {
            packageId = $"a_story_{dto.MainStorySequence}_template",
            version = "1.0",
            generatedAt = DateTime.UtcNow.ToString("o"),
            sceneTemplates = new List<SceneTemplateDTO> { dto }
        };

        // Serialize with pretty formatting for debugging
        string json = System.Text.Json.JsonSerializer.Serialize(package, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

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
            // No A-story progression yet, initialize for A11
            return AStoryContext.InitializeForProceduralGeneration();
        }

        // Reconstruct context from completed scenes
        Scene lastCompleted = completedAScenes.Last();
        int lastSequence = lastCompleted.MainStorySequence.Value;

        AStoryContext context = new AStoryContext
        {
            CurrentSequence = lastSequence + 1,
            LastCompletedSequence = lastSequence,
            CompletedASceneIds = completedAScenes.Select(s => s.Id).ToList(),
            RecentArchetypeIds = new List<string>(),
            RecentRegionIds = new List<string>(),
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
            // Extract archetype from template (if available)
            if (scene.Template != null && !string.IsNullOrEmpty(scene.Template.SceneArchetypeId))
            {
                context.RecentArchetypeIds.Add(scene.Template.SceneArchetypeId);
            }

            // Extract region from placement location
            if (!string.IsNullOrEmpty(scene.PlacementId))
            {
                // RegionId removed from Location - track by VenueId instead
                Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == scene.PlacementId);
                if (location != null && !string.IsNullOrEmpty(location.VenueId))
                {
                    if (!context.RecentRegionIds.Contains(location.VenueId))
                    {
                        context.RecentRegionIds.Add(location.VenueId);
                    }
                }
            }

            // Extract NPC personality from scene placement (if NPC-placed)
            // For A-story scenes, typically Location-placed, but check anyway
            if (scene.PlacementType == PlacementType.NPC && !string.IsNullOrEmpty(scene.PlacementId))
            {
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == scene.PlacementId);
                if (npc != null && !context.RecentPersonalityTypes.Contains(npc.PersonalityType))
                {
                    context.RecentPersonalityTypes.Add(npc.PersonalityType);
                }
            }
        }

        return context;
    }
}
