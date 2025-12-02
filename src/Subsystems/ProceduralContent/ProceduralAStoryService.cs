/// <summary>
/// Service for generating procedural A-story scene templates.
/// Creates infinite main story progression after authored tutorial completes.
/// Works from ANY sequence - if tutorial is A1-A3, generates A4+.
///
/// ARCHITECTURE PATTERN: Dynamic Template Package (HIGHLANDER-Compliant)
/// 1. Select archetype based on rhythm phase + location context + history
/// 2. Generate SceneTemplateDTO with categorical properties
/// 3. Serialize to JSON package
/// 4. Load via PackageLoaderFacade (JSON → PackageLoader → Parser → Entity)
/// 5. Template added to GameWorld.SceneTemplates, available for spawning
///
/// INTEGRATION POINT: Called from SpawnService when A-story scene completes.
/// Detection: scene.Category == MainStory && scene.MainStorySequence.HasValue
/// Trigger: Generate MainStorySequence + 1 template if not already exists
///
/// ARCHETYPE SELECTION STRATEGY (HISTORY-DRIVEN, gdd/01 §1.8):
/// - Selection based on intensity history, rhythm phase, location context
/// - Avoid recent archetypes (anti-repetition window)
/// - Match tier escalation (personal → local → regional, grounded character-driven)
/// - Current player state (Resolve, stats) NEVER influences selection
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
    private readonly PackageLoader _packageLoader;

    public ProceduralAStoryService(
        GameWorld gameWorld,
        ContentGenerationFacade contentFacade,
        PackageLoader packageLoader)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _contentFacade = contentFacade ?? throw new ArgumentNullException(nameof(contentFacade));
        _packageLoader = packageLoader ?? throw new ArgumentNullException(nameof(packageLoader));
    }

    /// <summary>
    /// Generate next A-story template for given sequence.
    /// Called when previous A-scene completes.
    /// Returns generated template ID (for tracking).
    ///
    /// CONTEXT INJECTION (HIGHLANDER, arc42 §8.28):
    /// - Receives SceneSelectionInputs (complete categorical inputs)
    /// - SAME selection logic for authored and procedural
    /// - Selection based on rhythm phase + location context + history
    /// - Current player state NEVER influences selection
    ///
    /// CATALOGUE PATTERN: Uses categorical properties (ArchetypeCategory, ExcludedArchetypes).
    /// Parser resolves to specific archetype via Catalogue at PARSE TIME.
    /// NO runtime catalogue calls - all resolution through Parser pipeline.
    ///
    /// CHALLENGE PHILOSOPHY: Player RESOURCES do NOT affect category selection.
    /// Fair rhythm emerges from story structure and location context.
    /// Peaceful is earned through intensity history, not granted when player struggles.
    /// See gdd/06_balance.md §6.8 for Challenge and Consequence Philosophy.
    /// </summary>
    public async Task<string> GenerateNextATemplate(
        int sequence,
        AStoryContext context,
        SceneSelectionInputs selectionInputs)
    {
        // Get archetype CATEGORY from selection inputs
        // HIGHLANDER: Same selection logic for authored and procedural
        // Selection based on rhythm phase + location context
        string archetypeCategory = SelectArchetypeCategory(selectionInputs);

        // Get excluded archetypes for anti-repetition (categorical property)
        List<string> excludedArchetypes = GetExcludedArchetypes(context);

        // Build SceneTemplateDTO with categorical properties
        SceneTemplateDTO dto = BuildSceneTemplateDTO(sequence, archetypeCategory, excludedArchetypes, context, selectionInputs);

        // Serialize to JSON package
        string packageJson = SerializeTemplatePackage(dto);
        string packageId = $"a_story_{sequence}_template";

        // Write dynamic package file
        await _contentFacade.CreateDynamicPackageFile(packageJson, packageId);

        // Load through HIGHLANDER pipeline (JSON → PackageLoader → Parser)
        // Result discarded - templates don't need post-load configuration
        _ = await _packageLoader.LoadDynamicPackageFromJson(packageJson, packageId);

        return dto.Id;
    }

    /// <summary>
    /// Select archetype category from SceneSelectionInputs.
    /// HIGHLANDER: Same logic for authored and procedural content.
    /// The only difference is WHERE inputs come from, not HOW they're processed.
    ///
    /// SIMPLIFIED (arc42 §8.28):
    /// - RhythmPattern is THE ONLY driver for category selection
    /// - Anti-repetition prevents same category twice
    /// - LocationSafety/Purpose/Tier REMOVED (legacy)
    /// - Current player state NEVER influences selection
    /// </summary>
    private string SelectArchetypeCategory(SceneSelectionInputs inputs)
    {
        // Determine appropriate categories based on rhythm pattern ONLY
        List<string> appropriateCategories = GetCategoriesForRhythmPattern(inputs.RhythmPattern);

        // Apply anti-repetition (avoid recent categories)
        List<string> availableCategories = appropriateCategories
            .Where(c => !inputs.RecentCategories.Contains(c))
            .ToList();

        // If all filtered out, use appropriate categories without anti-repetition
        if (!availableCategories.Any())
        {
            availableCategories = appropriateCategories;
        }

        // Deterministic selection using RhythmPattern as categorical property
        // Pattern ordinal selects different indices: Building(0), Crisis(1), Mixed(2)
        int selectionIndex = ((int)inputs.RhythmPattern) % availableCategories.Count;
        return availableCategories[selectionIndex];
    }

    /// <summary>
    /// Get categories appropriate for the current rhythm pattern.
    /// </summary>
    private List<string> GetCategoriesForRhythmPattern(RhythmPattern pattern)
    {
        return pattern switch
        {
            // Building: favor building opportunities (accumulation phase)
            RhythmPattern.Building => new List<string> { "Investigation", "Social", "Confrontation" },

            // Crisis: time for crisis/challenge (test phase)
            RhythmPattern.Crisis => new List<string> { "Crisis", "Confrontation" },

            // Mixed: gentle scenes after crisis (recovery phase)
            RhythmPattern.Mixed => new List<string> { "Social", "Investigation" },

            _ => new List<string> { "Investigation", "Social", "Confrontation", "Crisis" }
        };
    }

    /// <summary>
    /// Get excluded archetypes for anti-repetition (categorical properties).
    /// Returns list of archetype NAMES - Parser will use these when resolving via Catalogue.
    /// </summary>
    private List<string> GetExcludedArchetypes(AStoryContext context)
    {
        return context.RecentArchetypes
            .Select(a => a.ToString())
            .ToList();
    }


    /// <summary>
    /// Build SceneTemplateDTO for procedural A-story scene.
    /// Uses categorical properties (no concrete entity IDs at generation time).
    /// PlacementFilter uses Generic relation for runtime resolution.
    ///
    /// CATALOGUE PATTERN: Uses ArchetypeCategory + ExcludedArchetypes (categorical).
    /// Parser will resolve to specific SceneArchetypeType via Catalogue at parse-time.
    ///
    /// CONTEXT INJECTION (HIGHLANDER): Uses SceneSelectionInputs for rhythm determination.
    /// arc42 §8.28: Tier REMOVED - difficulty scaling via Location.Difficulty at choice generation.
    /// </summary>
    private SceneTemplateDTO BuildSceneTemplateDTO(
        int sequence,
        string archetypeCategory,
        List<string> excludedArchetypes,
        AStoryContext context,
        SceneSelectionInputs selectionInputs)
    {
        string sceneId = $"a_story_{sequence}";

        // Build categorical placement filter (may use target location context)
        PlacementFilterDTO placementFilter = BuildPlacementFilter(sequence, context);

        // Build spawn conditions (A-scenes spawn automatically via completion trigger)
        SpawnConditionsDTO spawnConditions = BuildSpawnConditions(sequence, context);

        // Determine rhythm pattern based on archetype category and input rhythm
        // Sir Brante Pattern: Building accumulates capability, Crisis tests it
        // HIGHLANDER: Uses RhythmPattern from inputs, not player state
        string rhythmPattern = DetermineRhythmPatternForScene(archetypeCategory, selectionInputs.RhythmPattern);

        SceneTemplateDTO dto = new SceneTemplateDTO
        {
            Id = sceneId,
            Archetype = "Linear", // A-story scenes are linear progression
            DisplayNameTemplate = $"The Path Deepens (A{sequence})", // AI will generate better title
            // CATALOGUE PATTERN: Use ArchetypeCategory instead of explicit SceneArchetype
            // Parser calls SceneArchetypeCatalog.ResolveFromCategory at parse-time
            ArchetypeCategory = archetypeCategory,
            ExcludedArchetypes = excludedArchetypes,
            LocationActivationFilter = placementFilter, // A-story activates when player enters matching location
            SpawnConditions = spawnConditions,
            Category = "MainStory",
            MainStorySequence = sequence,
            PresentationMode = "Modal", // A-story takes over screen (Sir Brante pattern)
            ProgressionMode = "Cascade", // Situations flow with momentum
            IsStarter = false, // Procedural A-story spawns from reward, not at game start
            ExpirationDays = null, // A-story never expires
            IntroNarrativeTemplate = null, // AI generates from hints
            RhythmPattern = rhythmPattern // Sir Brante rhythm (Building/Crisis/Mixed)
        };

        return dto;
    }

    /// <summary>
    /// Determine final rhythm pattern for scene based on archetype category and input rhythm.
    /// Sir Brante Pattern (arc42 §8.26): Building accumulates capability, Crisis tests it.
    ///
    /// CONTEXT INJECTION (HIGHLANDER):
    /// - Peaceful category → Building rhythm (all positive, earned structural respite)
    /// - Crisis category → Crisis rhythm (test player investments, penalty on fallback)
    /// - Mixed input → Building rhythm for recovery
    /// - Other combinations → Mixed rhythm (standard trade-offs)
    ///
    /// Uses RhythmPattern from inputs (computed from intensity history by caller).
    /// </summary>
    private string DetermineRhythmPatternForScene(string archetypeCategory, RhythmPattern inputRhythm)
    {
        // Peaceful category always uses Building rhythm (earned respite, all positive)
        if (archetypeCategory == "Peaceful")
        {
            return "Building";
        }

        // Crisis category always uses Crisis rhythm (test investments)
        if (archetypeCategory == "Crisis")
        {
            return "Crisis";
        }

        // Mixed input rhythm uses Building for recovery
        // HIGHLANDER: Uses RhythmPattern from inputs, not player state
        if (inputRhythm == RhythmPattern.Mixed)
        {
            return "Building";
        }

        // All other categories use Mixed rhythm (standard trade-offs)
        return "Mixed";
    }

    /// <summary>
    /// Build categorical placement filter for A-story scene.
    /// arc42 §8.28: Uses CATEGORICAL properties only - no specific entity selection.
    /// arc42 §8.3: No entity IDs, no sequence-based selection - EntityResolver resolves at runtime.
    /// Procedural A-story spawns via SceneSpawnReward, not location entry.
    /// </summary>
    private PlacementFilterDTO BuildPlacementFilter(int sequence, AStoryContext context)
    {
        // arc42 §8.28: Procedural A-story uses minimal categorical constraints
        // EntityResolver finds matching locations/NPCs at spawn time
        // NO specific region or personality selection - that violates categorical principle
        PlacementFilterDTO filter = new PlacementFilterDTO
        {
            PlacementType = "Location", // A-story happens at locations

            // Location filters: CATEGORICAL only (no specific region)
            // RegionName = null means any region is acceptable
            // EntityResolver finds matching location at spawn time

            // NPC filters: CATEGORICAL properties
            // NpcTags provides categorical constraint for NPC selection
            MinBond = null, // A-story accessible regardless of relationships
            MaxBond = null,
            NpcTags = new List<string> { "order_connected" }
        };

        return filter;
    }

    // DELETED: SelectRegion, SelectPersonalityType, SelectLocationCapabilities
    // arc42 §8.3: No sequence-based entity selection
    // arc42 §8.28: Selection uses categorical properties only
    // DDR-007: No Random in strategic layer
    // EntityResolver handles runtime resolution from categorical PlacementFilter

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
            RecentArchetypes = new List<SceneArchetypeType>(),
            RecentRegions = new List<Region>(),
            RecentPersonalityTypes = new List<PersonalityType>(),
            UnlockedRegionNames = new List<string>(),
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
            // SceneArchetype is now non-nullable (all scenes have an archetype)
            // PRINCIPLE: Archetype is a TYPE discriminator, not an ID (arc42 §8.3)
            context.RecentArchetypes.Add(scene.Template.SceneArchetype);

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
