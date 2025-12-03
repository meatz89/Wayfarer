using System.Text.Json;

/// <summary>
/// Orchestrates loading of game packages, delegating to specialized parsers for conversion.
/// Uses simple sequential loading in dependency order for static content.
///
/// ARCHITECTURAL PRINCIPLES:
/// - GameWorld is the SINGLE SOURCE OF TRUTH: All loaded content goes directly into GameWorld
/// - NO INTERMEDIATE STATE: Parsers convert JSON to domain objects immediately, no pass-through
/// - DEPENDENCY ORDER: Files must be numbered (01_, 02_, etc) to ensure proper loading sequence
/// - STATIC vs DYNAMIC: Static content loaded at startup (no skeletons), dynamic content supports skeletons
/// - PARSERS ARE STATELESS: Used only for conversion, then discarded - no state retention
/// </summary>
public class PackageLoader
{
    private readonly GameWorld _gameWorld;
    private bool _isFirstPackage = true;
    private string _currentPackageId = "unknown"; // Track current package for error reporting

    // Track loaded packages to prevent reloading
    private List<string> _loadedPackageIds = new List<string>();

    // Required for CreateSingleLocation (public method used by EntityResolver)
    private readonly LocationPlacementService _locationPlacementService;

    // COMPOSITION OVER INHERITANCE: Specialized loaders extracted from PackageLoader
    private readonly TravelSystemLoader _travelLoader;
    private readonly SceneSituationSystemLoader _sceneSituationLoader;
    private readonly CardSystemLoader _cardLoader;
    private readonly SpatialHierarchyLoader _spatialHierarchyLoader;
    private readonly SpatialPlacementLoader _spatialPlacementLoader;

    public PackageLoader(
        GameWorld gameWorld,
        SceneGenerationFacade sceneGenerationFacade,
        LocationPlayabilityValidator locationValidator,
        LocationPlacementService locationPlacementService)
    {
        _gameWorld = gameWorld;
        _locationPlacementService = locationPlacementService;

        // Initialize specialized loaders
        _travelLoader = new TravelSystemLoader(gameWorld);
        _sceneSituationLoader = new SceneSituationSystemLoader(gameWorld, sceneGenerationFacade);
        _cardLoader = new CardSystemLoader(gameWorld);
        _spatialHierarchyLoader = new SpatialHierarchyLoader(gameWorld, locationValidator, locationPlacementService);
        _spatialPlacementLoader = new SpatialPlacementLoader(gameWorld, locationValidator, locationPlacementService);
    }

    /// <summary>
    /// HIGHLANDER: THE ONLY method for location creation.
    /// Used by:
    /// 1. Static JSON parsing (foreach location in package.locations → CreateSingleLocation)
    /// 2. Runtime creation via EntityResolver.FindOrCreateLocation()
    ///
    /// Single path ensures consistent venue assignment, hex positioning, and GameWorld registration.
    /// </summary>
    public Location CreateSingleLocation(LocationDTO dto, Venue venue)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "LocationDTO cannot be null");
        if (venue == null)
            throw new ArgumentNullException(nameof(venue), "Venue cannot be null - all locations must belong to a venue");

        // Parse DTO to Location domain entity
        Location location = LocationParser.ConvertDTOToLocation(dto, _gameWorld);

        // Assign venue (HIGHLANDER: single assignment point)
        location.AssignVenue(venue);

        // Find unoccupied hex in venue cluster (HIGHLANDER: single hex assignment point)
        // CRITICAL: Don't blindly assign center hex - it may already be occupied
        AxialCoordinates? availableHex = _locationPlacementService.FindUnoccupiedHexInVenue(venue);

        if (!availableHex.HasValue)
        {
            throw new InvalidOperationException(
                $"Cannot create location '{location.Name}' in venue '{venue.Name}': " +
                $"No unoccupied hexes available. Venue capacity: {venue.MaxLocations}, " +
                $"Allocation: {venue.HexAllocation}. Consider increasing venue capacity or using a larger hex allocation.");
        }

        location.HexPosition = availableHex.Value;

        // Calculate difficulty from hex distance to world center
        // HIGHLANDER: Same calculation as PlaceLocationsInVenue
        _locationPlacementService.CalculateLocationDifficulty(location);

        // Register in GameWorld
        _gameWorld.Locations.Add(location);

        // HIGHLANDER: Location.HexPosition is source of truth - no derived sync needed

        Console.WriteLine($"[PackageLoader] CreateSingleLocation: Created '{location.Name}' in venue '{venue.Name}' at hex ({availableHex.Value.Q}, {availableHex.Value.R})");

        // RUNTIME ACTION REGENERATION: Generate intra-venue movement actions for dynamically created locations
        // This ensures scene-created locations have proper movement actions both TO and FROM neighboring locations
        List<LocationAction> newActions = LocationActionCatalog.RegenerateIntraVenueActionsForNewLocation(location, _gameWorld.Locations);
        _gameWorld.LocationActions.AddRange(newActions);
        Console.WriteLine($"[PackageLoader] CreateSingleLocation: Added {newActions.Count} intra-venue movement actions for '{location.Name}'");

        return location;
    }

    /// <summary>
    /// HIGHLANDER: THE ONLY method for NPC creation.
    /// Used by:
    /// 1. Static JSON parsing (foreach npc in package.npcs → CreateSingleNpc)
    /// 2. Runtime creation via EntityResolver.FindOrCreateNPC()
    ///
    /// Single path ensures consistent location assignment and GameWorld registration.
    /// </summary>
    public NPC CreateSingleNpc(NPCDTO dto, Location location)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "NPCDTO cannot be null");

        // Create EntityResolver for NPC parsing (find-only, global search for parse-time)
        EntityResolver entityResolver = new EntityResolver(_gameWorld);

        // Parse DTO to NPC domain entity
        NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld, entityResolver);

        // Assign location if provided (overrides any location from DTO)
        if (location != null)
        {
            npc.Location = location;
        }

        // Register in GameWorld
        _gameWorld.NPCs.Add(npc);

        Console.WriteLine($"[PackageLoader] CreateSingleNpc: Created '{npc.Name}' at location '{location?.Name ?? "none"}'");

        return npc;
    }

    /// <summary>
    /// HIGHLANDER: THE ONLY method for Scene creation.
    /// Used by:
    /// 1. Static JSON parsing (foreach scene in package.scenes → CreateSingleScene) - currently disabled
    /// 2. Runtime creation via SceneInstantiator.CreateDeferredScene()
    ///
    /// Single path ensures consistent template resolution and GameWorld registration.
    /// </summary>
    public Scene CreateSingleScene(SceneDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "SceneDTO cannot be null");

        // Parse DTO to Scene domain entity
        Scene scene = SceneParser.ConvertDTOToScene(dto, _gameWorld);

        // Register in GameWorld
        _gameWorld.Scenes.Add(scene);

        Console.WriteLine($"[PackageLoader] CreateSingleScene: Created '{scene.DisplayName}' (State={scene.State})");

        return scene;
    }

    /// <summary>
    /// Load static packages in alphabetical/numerical order
    /// Used at game startup for deterministic content loading
    /// PACKAGE-ROUND TRACKING: Accumulates results from all packages, initializes spatial systems ONCE
    /// </summary>
    public void LoadStaticPackages(List<string> packageFilePaths)
    {
        // Sort by filename to ensure proper loading order (01_, 02_, etc.)
        List<string> sortedPackages = packageFilePaths
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        // Accumulate results from all packages
        List<PackageLoadResult> allResults = new List<PackageLoadResult>();

        // Load each package sequentially
        foreach (string packagePath in sortedPackages)
        {
            string packageFileName = Path.GetFileName(packagePath);
            Console.WriteLine($"[PackageLoader] Loading package: {packageFileName}");

            string json = File.ReadAllText(packagePath);
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            // Check if already loaded
            if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
            {
                continue;
            }

            // Track as loaded
            if (package.PackageId != null)
            {
                _loadedPackageIds.Add(package.PackageId);
            }

            // Load with no skeletons allowed for static content, accumulate result
            PackageLoadResult result = LoadPackageContent(package, allowSkeletons: false);
            allResults.Add(result);
        }

        // Aggregate all entities across all packages
        List<Venue> allVenues = allResults.SelectMany(r => r.VenuesAdded).ToList();
        List<Location> allLocations = allResults.SelectMany(r => r.LocationsAdded).ToList();

        Console.WriteLine($"[PackageLoader] Aggregated {allVenues.Count} venues and {allLocations.Count} locations from {allResults.Count} packages");

        // COMPOSITION OVER INHERITANCE: Delegate spatial placement to specialized loader
        // Handles: venue placement, location placement, hex sync, grid completeness
        _spatialPlacementLoader.PlaceAndValidateSpatialEntities(allVenues, allLocations);

        // CATALOGUE PATTERN: Generate content from loaded entities (ONCE after all packages loaded)
        GeneratePlayerActionsFromCatalogue();
        GenerateLocationActionsFromCatalogue();
        _spatialPlacementLoader.GenerateProceduralRoutes();
        GenerateDeliveryJobsFromCatalogue();

        // PLAYER INITIALIZATION: Apply initial player configuration AFTER all content loaded
        _gameWorld.ApplyInitialPlayerConfiguration();

        // PLAYABILITY VALIDATION: Validate locations AFTER routes generated
        _spatialPlacementLoader.ValidateAllLocations();

        // Final validation and initialization
        PackageLoaderValidation.ValidateCrossroadsConfiguration(_gameWorld);
        _travelLoader.InitializeTravelDiscoverySystem();
        InitializeObligationJournal();
    }

    /// <summary>
    /// Load package content with optional skeleton support
    /// Returns PackageLoadResult tracking all entities added during THIS package load round
    /// COMPOSITION OVER INHERITANCE: Delegates to specialized loaders
    /// </summary>
    private PackageLoadResult LoadPackageContent(Package package, bool allowSkeletons)
    {
        // Create result to track entities from THIS round
        PackageLoadResult result = new PackageLoadResult { PackageId = package.PackageId ?? "unknown" };

        // Set current package ID for error reporting
        if (string.IsNullOrEmpty(package.PackageId))
            throw new InvalidDataException("Package missing required field 'PackageId'");
        _currentPackageId = package.PackageId;

        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        if (package.Content == null) return result;

        // Load in strict dependency order
        // 1. Foundation entities (no dependencies)
        LoadHexMap(package.Content.HexMap, allowSkeletons);
        LoadPlayerStatsConfiguration(package.Content.PlayerStatsConfig, allowSkeletons);
        LoadListenDrawCounts(package.Content.ListenDrawCounts);

        // 2. Spatial hierarchy (Regions → Districts → Venues → Locations)
        // DOMAIN COLLECTION PRINCIPLE: Uses LINQ queries instead of Dictionary lookups
        _spatialHierarchyLoader.LoadSpatialContent(
            package.Content.Regions,
            package.Content.Districts,
            package.Content.Venues,
            package.Content.Locations,
            result,
            allowSkeletons);

        LoadItems(package.Content.Items, result, allowSkeletons);

        // 3. Cards and tactical systems
        _cardLoader.LoadCardContent(
            package.Content.SocialCards,
            package.Content.MentalCards,
            package.Content.PhysicalCards,
            package.Content.SocialChallengeDecks,
            package.Content.MentalChallengeDecks,
            package.Content.PhysicalChallengeDecks,
            package.Content.Obligations,
            package.Content.Exchanges,
            result,
            allowSkeletons);

        // 4. Scene-Situation architecture (UNIFIED PATH: SceneTemplates only, no Scene instances)
        _sceneSituationLoader.LoadSceneSituationContent(
            package.Content.States,
            package.Content.Achievements,
            package.Content.SceneTemplates,
            package.Content.ConversationTrees,
            package.Content.ObservationScenes,
            package.Content.EmergencySituations,
            package.Content.Situations,
            result,
            allowSkeletons);

        // 5. NPCs (reference locations and cards)
        LoadNPCs(package.Content.Npcs, result, allowSkeletons);
        LoadStrangers(package.Content.Strangers, allowSkeletons);

        // 6. Travel content
        _travelLoader.LoadTravelContent(
            package.Content.PathCards,
            package.Content.EventCards,
            package.Content.TravelEvents,
            package.Content.PathCardCollections,
            package.Content.Routes,
            result,
            allowSkeletons);

        // 7. NPC deck initialization (depends on NPCs and exchanges)
        _cardLoader.InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 8. Complex entities
        LoadDialogueTemplates(package.Content.DialogueTemplates, allowSkeletons);
        LoadStandingObligations(package.Content.StandingObligations, allowSkeletons);

        return result;
    }

    /// <summary>
    /// Load all packages from a directory with proper ordering
    /// </summary>
    public void LoadPackagesFromDirectory(string directoryPath)
    {
        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .ToList();

        // Use simple static loading for game start
        LoadStaticPackages(packageFiles);
    }

    /// <summary>
    /// Load a dynamic package at runtime (e.g., AI-generated content)
    /// Returns list of skeleton IDs that need completion
    /// </summary>
    public List<string> LoadDynamicPackage(string packageFilePath)
    {
        string json = File.ReadAllText(packageFilePath);
        Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        });

        // Check if already loaded
        if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
        {
            return new List<string>();
        }

        // Track as loaded
        if (package.PackageId != null)
        {
            _loadedPackageIds.Add(package.PackageId);
        }

        // Load with skeletons allowed for dynamic content
        LoadPackageContent(package, allowSkeletons: true);

        // Return skeleton IDs for AI completion
        return _gameWorld.SkeletonRegistry.Select(r => r.SkeletonKey).ToList();
    }

    /// <summary>
    /// Load a dynamic package from JSON string for TEMPLATE loading only.
    ///
    /// IMPORTANT DISTINCTION (HIGHLANDER):
    /// - This method loads TEMPLATES (SceneTemplate, SituationTemplate) from procedurally-generated JSON
    /// - For SCENE INSTANCES, use CreateSingleScene(SceneDTO) instead
    ///
    /// Used by:
    /// - ProceduralAStoryService.GenerateNextATemplate() - generates SceneTemplateDTO, loads as template
    /// - ContentGenerationFacade - procedural content template loading
    ///
    /// NOT used for:
    /// - Scene instance creation (use CreateSingleScene)
    /// - NPC/Location instance creation (use CreateSingleNpc/CreateSingleLocation)
    ///
    /// This method exists because procedural templates require the full Parser pipeline
    /// (JSON → DTO → Parser → Entity) to resolve categorical properties via Catalogues.
    /// Templates are immutable archetypes, not game state.
    /// </summary>
    public async Task<PackageLoadResult> LoadDynamicPackageFromJson(string json, string packageId)
    {
        Package package;
        using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
        {
            package = await JsonSerializer.DeserializeAsync<Package>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            });
        }

        // Set package ID if not present
        if (string.IsNullOrEmpty(package.PackageId))
        {
            if (string.IsNullOrEmpty(packageId))
                throw new InvalidDataException("Package and packageId parameter both missing - cannot identify package");
            package.PackageId = packageId;
        }

        // Check if already loaded
        if (_loadedPackageIds.Contains(package.PackageId))
        {
            return new PackageLoadResult { PackageId = package.PackageId };
        }

        // Track as loaded
        _loadedPackageIds.Add(package.PackageId);

        // Load with skeletons allowed for dynamic content, get result
        PackageLoadResult result = LoadPackageContent(package, allowSkeletons: true);

        Console.WriteLine($"[PackageLoader] Dynamic package loaded: {result.VenuesAdded.Count} venues, {result.LocationsAdded.Count} locations, {result.SceneTemplatesAdded.Count} scene templates");

        // COMPOSITION OVER INHERITANCE: Delegate spatial placement to specialized loader
        _spatialPlacementLoader.PlaceAndValidateSpatialEntities(result.VenuesAdded, result.LocationsAdded);

        // Regenerate static location actions for dynamic packages
        GenerateLocationActionsFromCatalogue();

        // Return full result with direct object references to created entities
        return result;
    }

    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config - STORE CONFIG ONLY, apply after all packages loaded
        if (conditions.PlayerConfig != null)
        {
            // Parse DTO (categorical properties) → Domain Entity (concrete values)
            PlayerInitialConfig parsedConfig = PlayerInitialConfigParser.Parse(conditions.PlayerConfig);
            _gameWorld.InitialPlayerConfig = parsedConfig;
            // NOTE: ApplyInitialPlayerConfiguration() moved to LoadStaticPackages()
            // REASON: Player config references Items, which must be loaded first
            // Player initialization is GAME STARTUP phase, not PACKAGE LOADING phase
        }

        // Set starting location - CRITICAL for playability
        if (string.IsNullOrEmpty(conditions.StartingSpotId))
            throw new InvalidOperationException("StartingSpotId is required in starting conditions - player has no spawn location!");

        // Validate starting location exists in parsed locations
        // HIGHLANDER: LINQ query on already-parsed locations
        Location startingLocation = _gameWorld.Locations.FirstOrDefault(l => l.Name == conditions.StartingSpotId);
        if (startingLocation == null)
            throw new InvalidOperationException($"StartingSpotId '{conditions.StartingSpotId}' not found in parsed locations - player cannot spawn!");

        // HIGHLANDER: Store starting location object reference in GameWorld
        // GameOrchestrator.StartGameAsync uses this to initialize Player.CurrentPosition
        _gameWorld.StartingLocation = startingLocation;

        // Apply starting obligations
        if (conditions.StartingObligations != null)
        {
            foreach (StandingObligationDTO obligationDto in conditions.StartingObligations)
            {
                // Convert DTO to domain model and add to player's standing obligations
                StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(obligationDto, _gameWorld);
                _gameWorld.GetPlayer().StandingObligations.Add(obligation);
            }
        }

        // Apply starting token relationships
        // DOMAIN COLLECTION PRINCIPLE: List<NpcTokenStartEntry> instead of Dictionary
        if (conditions.StartingTokens != null)
        {
            foreach (NpcTokenStartEntry entry in conditions.StartingTokens)
            {
                // HIGHLANDER: Resolve NPC name to NPC object
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.Name == entry.NpcId);
                if (npc != null)
                {
                    // HIGHLANDER: Set tokens directly on NPC
                    npc.Trust = entry.Tokens.Trust;
                    npc.Diplomacy = entry.Tokens.Diplomacy;
                    npc.Status = entry.Tokens.Status;
                    npc.Shadow = entry.Tokens.Shadow;
                }
            }
        }

        // Store time initialization for TimeModel (applied after DI initialization)
        if (conditions.StartingDay.HasValue)
        {
            _gameWorld.InitialDay = conditions.StartingDay.Value;
        }

        if (!string.IsNullOrEmpty(conditions.StartingTimeBlock))
        {
            if (Enum.TryParse<TimeBlocks>(conditions.StartingTimeBlock, out TimeBlocks timeBlock))
            {
                _gameWorld.InitialTimeBlock = timeBlock;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid StartingTimeBlock '{conditions.StartingTimeBlock}'. " +
                    $"Valid values: Morning, Midday, Afternoon, Evening");
            }
        }

        if (conditions.StartingSegment.HasValue)
        {
            _gameWorld.InitialSegment = conditions.StartingSegment.Value;
        }
    }

    /// <summary>
    /// Generate universal player actions from PlayerActionCatalog (PARSE TIME ONLY)
    /// Called ONCE after all locations loaded, not per location
    /// CATALOGUE PATTERN: Actions generated from categorical properties, never from JSON
    /// </summary>
    private void GeneratePlayerActionsFromCatalogue()
    {
        // Generate universal player actions (CheckBelongings, Wait, SleepOutside)
        List<PlayerAction> playerActions = PlayerActionCatalog.GenerateUniversalActions();

        // Add to GameWorld (check for duplicates if multiple packages)
        foreach (PlayerAction action in playerActions)
        {
            // Avoid duplicates if multiple packages loaded (object equality check)
            if (!_gameWorld.PlayerActions.Any(a => a == action))
            {
                _gameWorld.PlayerActions.Add(action);
            }
        }
    }

    /// <summary>
    /// Generate location actions from LocationActionCatalog (PARSE TIME ONLY)
    /// Called ONCE after all locations loaded (needs complete location list for intra-venue movement)
    /// CATALOGUE PATTERN: Actions generated from categorical properties, never from JSON
    /// </summary>
    private void GenerateLocationActionsFromCatalogue()
    {
        // Get all locations for intra-venue movement calculation
        List<Location> allLocations = _gameWorld.Locations.ToList();

        // Generate actions for each location based on its categorical properties
        foreach (Location location in allLocations)
        {
            List<LocationAction> generatedActions = LocationActionCatalog.GenerateActionsForLocation(
                location,
                allLocations // Pass all locations for intra-venue movement calculation
            );

            // Add generated actions to GameWorld
            foreach (LocationAction action in generatedActions)
            {
                // Avoid duplicates if multiple packages loaded (semantic deduplication via object references)
                bool isDuplicate = _gameWorld.LocationActions.Any(a =>
                    a.ActionType == action.ActionType &&
                    a.SourceLocation == action.SourceLocation &&
                    a.DestinationLocation == action.DestinationLocation);

                if (!isDuplicate)
                {
                    _gameWorld.LocationActions.Add(action);
                }
            }
        }
    }

    /// <summary>
    /// Generate delivery jobs from DeliveryJobCatalog (PARSE TIME ONLY)
    /// Called ONCE after routes and locations loaded
    /// CATALOGUE PATTERN: Jobs generated procedurally from routes, never from JSON
    /// </summary>
    private void GenerateDeliveryJobsFromCatalogue()
    {
        // Get all routes and locations
        List<RouteOption> allRoutes = _gameWorld.Routes.ToList();
        List<Location> allLocations = _gameWorld.Locations.ToList();

        // Generate delivery jobs from routes
        List<DeliveryJob> generatedJobs = DeliveryJobCatalog.GenerateJobsFromRoutes(allRoutes, allLocations);

        // Add generated jobs to GameWorld
        foreach (DeliveryJob job in generatedJobs)
        {
            // Avoid duplicates if multiple packages loaded (object equality check)
            if (!_gameWorld.AvailableDeliveryJobs.Any(j => j == job))
            {
                _gameWorld.AvailableDeliveryJobs.Add(job);
            }
        }
    }

    private void LoadPlayerStatsConfiguration(PlayerStatsConfigDTO playerStatsConfig, bool allowSkeletons)
    {
        if (playerStatsConfig == null) return;

        // Parse the player stats configuration using PlayerStatParser
        PlayerStatsParseResult parseResult = PlayerStatParser.ParseStatsPackage(playerStatsConfig);

        // Store the configuration in GameWorld
        _gameWorld.PlayerStatDefinitions = parseResult.StatDefinitions;
        _gameWorld.StatProgression = parseResult.Progression;
    }

    private void LoadListenDrawCounts(List<ListenDrawCountEntry> listenDrawCounts)
    {
        if (listenDrawCounts == null) return;

        // DOMAIN COLLECTION PRINCIPLE: Switch on enum, map to explicit properties
        foreach (ListenDrawCountEntry entry in listenDrawCounts)
        {
            if (!Enum.TryParse<ConnectionState>(entry.ConnectionState, true, out ConnectionState state))
            {
                throw new Exception($"[PackageLoader] Invalid connection state in listenDrawCounts: '{entry.ConnectionState}'");
            }

            switch (state)
            {
                case ConnectionState.DISCONNECTED:
                    GameRules.StandardRuleset.DisconnectedDrawCount = entry.DrawCount;
                    break;
                case ConnectionState.GUARDED:
                    GameRules.StandardRuleset.GuardedDrawCount = entry.DrawCount;
                    break;
                case ConnectionState.NEUTRAL:
                    GameRules.StandardRuleset.NeutralDrawCount = entry.DrawCount;
                    break;
                case ConnectionState.RECEPTIVE:
                    GameRules.StandardRuleset.ReceptiveDrawCount = entry.DrawCount;
                    break;
                case ConnectionState.TRUSTING:
                    GameRules.StandardRuleset.TrustingDrawCount = entry.DrawCount;
                    break;
            }
        }

        GameRules.StandardRuleset.MarkListenDrawCountsLoaded();
    }

    private void LoadStrangers(List<StrangerNPCDTO> strangerDtos, bool allowSkeletons)
    {
        if (strangerDtos == null) return;

        foreach (StrangerNPCDTO dto in strangerDtos)
        {
            // Convert DTO to domain model using StrangerParser
            NPC stranger = StrangerParser.ConvertDTOToNPC(dto, _gameWorld);

            // Add stranger to the unified NPCs list
            _gameWorld.NPCs.Add(stranger);
        }
    }

    /// <summary>
    /// Load hex map grid - fundamental spatial scaffolding
    /// HEX-BASED TRAVEL SYSTEM: Loads world hex grid with terrain, danger, and location placement
    /// </summary>
    private void LoadHexMap(HexMapDTO hexMapDto, bool allowSkeletons)
    {
        if (hexMapDto == null) return;

        // Parse hex map using HexParser
        HexMap hexMap = HexParser.ParseHexMap(hexMapDto);

        // Store in GameWorld
        _gameWorld.WorldHexGrid = hexMap;
    }

    // NOTE: SyncLocationHexPositions, EnsureHexGridCompleteness, ValidateAllLocations, GenerateProceduralRoutes
    // EXTRACTED TO: SpatialPlacementLoader.cs

    // NOTE: LoadRegions, LoadDistricts, LoadLocations (venues/spots)
    // EXTRACTED TO: SpatialHierarchyLoader.cs (with Dictionary→LINQ fix)

    // NOTE: LoadSocialCards, LoadMentalCards, LoadPhysicalCards, LoadSocialChallengeDecks,
    // LoadMentalChallengeDecks, LoadPhysicalChallengeDecks, LoadObligations, LoadExchanges
    // EXTRACTED TO: CardSystemLoader.cs

    // NOTE: LoadRoutes, LoadPathCards, LoadEventCards, LoadTravelEvents, LoadEventCollections
    // EXTRACTED TO: TravelSystemLoader.cs

    // NOTE: LoadStates, LoadAchievements, LoadSceneTemplates, LoadScenes, LoadSituations,
    // LoadConversationTrees, LoadObservationScenes, LoadEmergencySituations
    // EXTRACTED TO: SceneSituationSystemLoader.cs

    private void LoadNPCs(List<NPCDTO> npcDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (npcDtos == null) return;

        // EntityResolver for categorical entity resolution (find-only, global search)
        EntityResolver entityResolver = new EntityResolver(_gameWorld);

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if this NPC already exists - UPDATE IN-PLACE (never remove)
            NPC? existing = _gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.Name);

            if (existing != null)
            {
                // Preserve persistent deck cards (player-earned cards must not be lost)
                List<ExchangeCard> preservedExchangeCards = existing.ExchangeDeck.ToList();

                // Parse full NPC from DTO to get updated properties
                NPC parsed = NPCParser.ConvertDTOToNPC(dto, _gameWorld, entityResolver);

                // UPDATE existing NPC properties in-place (preserve object identity)
                existing.Name = parsed.Name;
                existing.Description = parsed.Description;
                existing.Role = parsed.Role;
                existing.Profession = parsed.Profession;
                existing.Level = parsed.Level;
                existing.ConversationDifficulty = parsed.ConversationDifficulty;
                existing.PersonalityDescription = parsed.PersonalityDescription;
                existing.PersonalityType = parsed.PersonalityType;
                existing.ConversationModifier = parsed.ConversationModifier;
                existing.Location = parsed.Location; // Resolved via EntityResolver.FindOrCreate
                existing.IsSkeleton = false;

                // PRESERVE ExchangeDeck: Merge authored initial cards + preserved runtime cards
                existing.ExchangeDeck.Clear();
                existing.ExchangeDeck.AddRange(parsed.ExchangeDeck); // Authored initial cards
                existing.ExchangeDeck.AddRange(preservedExchangeCards); // Runtime cards preserved

                // Remove from skeleton registry if present
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }

                // Track updated NPC (skeleton replacement counts as "added" for THIS round)
                result.NPCsAdded.Add(existing);
            }
            else
            {
                // New NPC - add to collection AND track in result
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld, entityResolver);
                _gameWorld.NPCs.Add(npc);
                result.NPCsAdded.Add(npc);
            }
        }
    }

    private void LoadDialogueTemplates(DialogueTemplates dialogueTemplates, bool allowSkeletons)
    {
        if (dialogueTemplates == null) return; _gameWorld.DialogueTemplates = dialogueTemplates;
    }

    private void LoadItems(List<ItemDTO> itemDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (itemDtos == null) return;

        foreach (ItemDTO dto in itemDtos)
        {
            Item item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.Items.Add(item);
            result.ItemsAdded.Add(item);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos, bool allowSkeletons)
    {
        if (obligationDtos == null) return;

        foreach (StandingObligationDTO dto in obligationDtos)
        {
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto, _gameWorld);
            _gameWorld.StandingObligationTemplates.Add(obligation);
        }
    }

    /// <summary>
    /// Initialize obligation journal with all obligations as Potential (awaiting discovery triggers)
    /// Called AFTER all packages are loaded to ensure all obligations exist
    /// HIGHLANDER: Object references ONLY, no ID collections
    /// </summary>
    private void InitializeObligationJournal()
    {
        ObligationJournal obligationJournal = _gameWorld.ObligationJournal;

        obligationJournal.PotentialObligations.Clear();
        foreach (Obligation obligation in _gameWorld.Obligations)
        {
            obligationJournal.PotentialObligations.Add(obligation);
        }
    }

}