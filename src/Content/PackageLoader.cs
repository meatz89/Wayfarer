using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Helper class for exchange card lookups
/// </summary>
internal class ExchangeCardEntry
{
    public string Id { get; set; }
    public ExchangeCard Card { get; set; }
}

/// <summary>
/// Helper class for path card lookups
/// </summary>
internal class PathCardEntry
{
    public string Id { get; set; }
    public PathCardDTO Card { get; set; }
}

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
    private List<ExchangeCardEntry> _parsedExchangeCards;
    private string _currentPackageId = "unknown"; // Track current package for error reporting

    // Track loaded packages to prevent reloading
    private List<string> _loadedPackageIds = new List<string>();

    public PackageLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Load static packages in alphabetical/numerical order
    /// Used at game startup for deterministic content loading
    /// </summary>
    public void LoadStaticPackages(List<string> packageFilePaths)
    {
        Console.WriteLine("[PackageLoader] Starting static package loading...");

        // Sort by filename to ensure proper loading order (01_, 02_, etc.)
        List<string> sortedPackages = packageFilePaths
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        Console.WriteLine($"[PackageLoader] Loading {sortedPackages.Count} packages in order:");
        foreach (string? path in sortedPackages)
        {
            Console.WriteLine($"  - {Path.GetFileName(path)}");
        }

        // Load each package sequentially
        foreach (string packagePath in sortedPackages)
        {
            try
            {
                string json = File.ReadAllText(packagePath);
                Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                });

                // Check if already loaded
                if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
                {
                    Console.WriteLine($"[PackageLoader] Skipping already loaded package: {package.PackageId}");
                    continue;
                }

                Console.WriteLine($"[PackageLoader] Loading package: {Path.GetFileName(packagePath)}");

                // Track as loaded
                if (package.PackageId != null)
                {
                    _loadedPackageIds.Add(package.PackageId);
                }

                // Load with no skeletons allowed for static content
                LoadPackageContent(package, allowSkeletons: false);

                Console.WriteLine($"[PackageLoader] Successfully loaded: {Path.GetFileName(packagePath)}");
            }
            catch (Exception ex)
            {
                throw new Exception($"[PackageLoader] Failed to load package {packagePath}: {ex.Message}", ex);
            }
        }

        // Final validation and initialization
        ValidateCrossroadsConfiguration();
        InitializeTravelDiscoverySystem();
        InitializeInvestigationJournal();

        Console.WriteLine($"[PackageLoader] Static loading complete: {sortedPackages.Count} packages loaded");
    }

    /// <summary>
    /// Load package content with optional skeleton support
    /// </summary>
    private void LoadPackageContent(Package package, bool allowSkeletons)
    {
        // Set current package ID for error reporting
        _currentPackageId = package.PackageId ?? "unknown";

        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        if (package.Content == null) return;

        // Load in strict dependency order
        // 1. Foundation entities (no dependencies)
        LoadPlayerStatsConfiguration(package.Content.PlayerStatsConfig, allowSkeletons);
        LoadListenDrawCounts(package.Content.ListenDrawCounts);
        LoadRegions(package.Content.Regions, allowSkeletons);
        LoadDistricts(package.Content.Districts, allowSkeletons);
        LoadItems(package.Content.Items, allowSkeletons);

        // 2. Venues and Locations (may reference regions/districts)
        LoadLocations(package.Content.Venues, allowSkeletons);
        LoadLocationSpots(package.Content.Locations, allowSkeletons);

        // 3. Cards (foundation for NPCs and conversations)
        LoadSocialCards(package.Content.Cards, allowSkeletons);
        LoadMentalCards(package.Content.MentalCards, allowSkeletons);
        LoadPhysicalCards(package.Content.PhysicalCards, allowSkeletons);
        // THREE PARALLEL TACTICAL SYSTEMS - Decks only, no Types
        LoadSocialChallengeDecks(package.Content.SocialChallengeDecks, allowSkeletons);
        LoadMentalChallengeDecks(package.Content.MentalChallengeDecks, allowSkeletons);
        LoadPhysicalChallengeDecks(package.Content.PhysicalChallengeDecks, allowSkeletons);

        // 3.5 Investigation Templates (strategic multi-phase activities)
        LoadInvestigations(package.Content.Investigations, allowSkeletons);
        LoadKnowledge(package.Content.Knowledge, allowSkeletons);
        LoadGoals(package.Content.Goals, allowSkeletons);
        LoadObstacles(package.Content.Obstacles, allowSkeletons);

        // 4. NPCs (reference locations, Locations, and cards)
        LoadNPCs(package.Content.Npcs, allowSkeletons);
        LoadStrangers(package.Content.Strangers, allowSkeletons);

        // 5. Routes (reference Locations which now have VenueId set)
        LoadRoutes(package.Content.Routes, allowSkeletons);

        // 6. Relationship entities (depend on NPCs and cards)
        LoadExchanges(package.Content.Exchanges, allowSkeletons);
        InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 7. Complex entities
        LoadDialogueTemplates(package.Content.DialogueTemplates, allowSkeletons);
        LoadStandingObligations(package.Content.StandingObligations, allowSkeletons);
        LoadLocationActions(package.Content.LocationActions, allowSkeletons);

        // 8. Travel content
        List<PathCardEntry> pathCardLookup = LoadPathCards(package.Content.PathCards, allowSkeletons);
        List<PathCardEntry> eventCardLookup = LoadEventCards(package.Content.EventCards, allowSkeletons);
        LoadTravelEvents(package.Content.TravelEvents, eventCardLookup, allowSkeletons);
        LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup, allowSkeletons);

        // 9. V2 Investigation and Travel Systems
        LoadTravelObstacles(package.Content.TravelObstacles, allowSkeletons);
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
        Console.WriteLine($"[PackageLoader] Loading dynamic package: {packageFilePath}");

        try
        {
            string json = File.ReadAllText(packageFilePath);
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            });

            // Check if already loaded
            if (package.PackageId != null && _loadedPackageIds.Contains(package.PackageId))
            {
                Console.WriteLine($"[PackageLoader] Package {package.PackageId} already loaded");
                return new List<string>();
            }

            // Track as loaded
            if (package.PackageId != null)
            {
                _loadedPackageIds.Add(package.PackageId);
            }

            // Load with skeletons allowed for dynamic content
            LoadPackageContent(package, allowSkeletons: true);

            Console.WriteLine($"[PackageLoader] Dynamic package loaded with {_gameWorld.SkeletonRegistry.Count} skeletons");

            // Return skeleton IDs for AI completion
            return _gameWorld.SkeletonRegistry.Select(r => r.SkeletonKey).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"[PackageLoader] Failed to load dynamic package: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Load a dynamic package from JSON string (e.g., AI-generated content)
    /// Returns list of skeleton IDs that need completion
    /// </summary>
    public List<string> LoadDynamicPackageFromJson(string json, string packageId)
    {
        Console.WriteLine($"[PackageLoader] Loading dynamic package from JSON: {packageId ?? "unnamed"}");

        try
        {
            Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            });

            // Set package ID if not present
            if (string.IsNullOrEmpty(package.PackageId))
            {
                package.PackageId = packageId ?? $"dynamic_{DateTime.Now.Ticks}";
            }

            // Check if already loaded
            if (_loadedPackageIds.Contains(package.PackageId))
            {
                Console.WriteLine($"[PackageLoader] Package {package.PackageId} already loaded");
                return new List<string>();
            }

            // Track as loaded
            _loadedPackageIds.Add(package.PackageId);

            // Load with skeletons allowed for dynamic content
            LoadPackageContent(package, allowSkeletons: true);

            Console.WriteLine($"[PackageLoader] Dynamic package {package.PackageId} loaded with {_gameWorld.SkeletonRegistry.Count} skeletons");

            // Return skeleton IDs for AI completion
            return _gameWorld.SkeletonRegistry.Select(r => r.SkeletonKey).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"[PackageLoader] Failed to load dynamic package: {ex.Message}", ex);
        }
    }



    private void ApplyStartingConditions(PackageStartingConditions conditions)
    {
        // Apply player initial config
        if (conditions.PlayerConfig != null)
        {
            _gameWorld.InitialPlayerConfig = conditions.PlayerConfig;
            // Apply the initial configuration to the player immediately
            _gameWorld.ApplyInitialPlayerConfiguration();
        }

        // Set starting location
        if (!string.IsNullOrEmpty(conditions.StartingSpotId))
        {
            _gameWorld.InitialLocationSpotId = conditions.StartingSpotId;
        }

        // Apply starting obligations
        if (conditions.StartingObligations != null)
        {
            foreach (StandingObligationDTO obligationDto in conditions.StartingObligations)
            {
                // Convert DTO to domain model and add to player's standing obligations
                StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(obligationDto);
                _gameWorld.GetPlayer().StandingObligations.Add(obligation);
            }
        }

        // Apply starting token relationships
        if (conditions.StartingTokens != null)
        {
            foreach (KeyValuePair<string, NPCTokenRelationship> kvp in conditions.StartingTokens)
            {
                // Token relationships will be applied when NPCs are loaded
                // Store for later application
                NPCTokenEntry tokenEntry = _gameWorld.GetPlayer().NPCTokens.GetNPCTokenEntry(kvp.Key);
                tokenEntry.Trust = kvp.Value.Trust;
                tokenEntry.Diplomacy = kvp.Value.Diplomacy;
                tokenEntry.Status = kvp.Value.Status;
                tokenEntry.Shadow = kvp.Value.Shadow;
            }
        }
    }

    private void LoadPlayerStatsConfiguration(PlayerStatsConfigDTO playerStatsConfig, bool allowSkeletons)
    {
        if (playerStatsConfig == null) return;

        Console.WriteLine("[PackageLoader] Loading player stats configuration...");

        // Parse the player stats configuration using PlayerStatParser
        PlayerStatsParseResult parseResult = PlayerStatParser.ParseStatsPackage(playerStatsConfig);

        // Store the configuration in GameWorld
        _gameWorld.PlayerStatDefinitions = parseResult.StatDefinitions;
        _gameWorld.StatProgression = parseResult.Progression;

        Console.WriteLine($"[PackageLoader] Loaded {parseResult.StatDefinitions.Count} stat definitions and progression configuration");
    }

    private void LoadListenDrawCounts(Dictionary<string, int> listenDrawCounts)
    {
        if (listenDrawCounts == null) return;

        Console.WriteLine("[PackageLoader] Loading listen draw counts...");

        // Convert string keys to ConnectionState enum and create ListenDrawCountEntry list
        List<ListenDrawCountEntry> drawCountEntries = new List<ListenDrawCountEntry>();

        foreach (KeyValuePair<string, int> kvp in listenDrawCounts)
        {
            // Parse connection state from string key
            if (Enum.TryParse<ConnectionState>(kvp.Key.ToUpper(), out ConnectionState state))
            {
                drawCountEntries.Add(new ListenDrawCountEntry
                {
                    State = state,
                    DrawCount = kvp.Value
                });
                Console.WriteLine($"[PackageLoader] Set draw count for {state}: {kvp.Value} cards");
            }
            else
            {
                throw new Exception($"[PackageLoader] Invalid connection state in listenDrawCounts: '{kvp.Key}'");
            }
        }

        // Apply to GameRules
        GameRules.StandardRuleset.ListenDrawCounts = drawCountEntries;
        Console.WriteLine($"[PackageLoader] Loaded {drawCountEntries.Count} listen draw count entries");
    }

    private void LoadStrangers(List<StrangerNPCDTO> strangerDtos, bool allowSkeletons)
    {
        if (strangerDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {strangerDtos.Count} stranger NPCs...");

        foreach (StrangerNPCDTO dto in strangerDtos)
        {
            // Convert DTO to domain model using StrangerParser
            NPC stranger = StrangerParser.ConvertDTOToNPC(dto);

            // Add stranger to the unified NPCs list
            _gameWorld.NPCs.Add(stranger);

            Console.WriteLine($"[PackageLoader] Added stranger '{stranger.Name}' (Level {stranger.Level}) to Venue '{stranger.Venue}' for time block '{stranger.AvailableTimeBlock}'");
        }

        Console.WriteLine($"[PackageLoader] Completed loading strangers. Total strangers loaded: {strangerDtos.Count}");
    }

    private void LoadRegions(List<RegionDTO> regionDtos, bool allowSkeletons)
    {
        if (regionDtos == null) return;

        foreach (RegionDTO dto in regionDtos)
        {
            Region region = new Region
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DistrictIds = dto.DistrictIds ?? new List<string>(),
                Government = dto.Government,
                Culture = dto.Culture,
                Population = dto.Population,
                MajorExports = dto.MajorExports ?? new List<string>(),
                MajorImports = dto.MajorImports ?? new List<string>()
            };
            _gameWorld.WorldState.Regions.Add(region);
        }
    }

    private void LoadDistricts(List<DistrictDTO> districtDtos, bool allowSkeletons)
    {
        if (districtDtos == null) return;

        foreach (DistrictDTO dto in districtDtos)
        {
            District district = new District
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                RegionId = dto.RegionId,
                VenueIds = dto.VenueIds ?? new List<string>(),
                DistrictType = dto.DistrictType,
                DangerLevel = dto.DangerLevel,
                Characteristics = dto.Characteristics ?? new List<string>()
            };
            _gameWorld.WorldState.Districts.Add(district);
        }
    }

    private void LoadSocialCards(List<SocialCardDTO> cardDtos, bool allowSkeletons)
    {
        if (cardDtos == null) return;

        foreach (SocialCardDTO dto in cardDtos)
        {
            // Use static method from ConversationCardParser
            SocialCard card = SocialCardParser.ParseCard(dto);
            _gameWorld.SocialCards.Add(card);
        }

        // Validate Foundation card rules after all cards are loaded
        List<SocialCard> allCards = _gameWorld.SocialCards.ToList();
        SocialCardParser.ValidateFoundationCardRules(allCards);
    }

    private void LoadMentalCards(List<MentalCardDTO> mentalCards, bool allowSkeletons)
    {
        if (mentalCards == null) return;

        Console.WriteLine($"[PackageLoader] Loading mental cards...");
        MentalCardParser parser = new MentalCardParser();
        foreach (MentalCardDTO dto in mentalCards)
        {
            MentalCard card = parser.ParseCard(dto);
            _gameWorld.MentalCards.Add(card);
            Console.WriteLine($"[PackageLoader] Loaded mental card '{card.Id}': {card.Name} (Depth {card.Depth})");
        }
        Console.WriteLine($"[PackageLoader] Completed loading mental cards. Total: {_gameWorld.MentalCards.Count}");
    }

    private void LoadPhysicalCards(List<PhysicalCardDTO> physicalCards, bool allowSkeletons)
    {
        if (physicalCards == null) return;

        Console.WriteLine($"[PackageLoader] Loading physical cards...");
        PhysicalCardParser parser = new PhysicalCardParser();
        foreach (PhysicalCardDTO dto in physicalCards)
        {
            PhysicalCard card = parser.ParseCard(dto);
            _gameWorld.PhysicalCards.Add(card);
            Console.WriteLine($"[PackageLoader] Loaded physical card '{card.Id}': {card.Name} (Depth {card.Depth})");
        }
        Console.WriteLine($"[PackageLoader] Completed loading physical cards. Total: {_gameWorld.PhysicalCards.Count}");
    }


    private void LoadSocialChallengeDecks(List<SocialChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        Console.WriteLine($"[PackageLoader] Loading conversation engagement decks...");
        foreach (SocialChallengeDeckDTO dto in decks)
        {
            SocialChallengeDeck deck = dto.ToDomain();
            _gameWorld.SocialChallengeDecks[deck.Id] = deck;
            Console.WriteLine($"[PackageLoader] Loaded conversation deck '{deck.Id}': {deck.Name} with {deck.CardIds.Count} cards");
        }
        Console.WriteLine($"[PackageLoader] Completed loading conversation engagement decks. Total: {_gameWorld.SocialChallengeDecks.Count}");
    }


    private void LoadMentalChallengeDecks(List<MentalChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        Console.WriteLine($"[PackageLoader] Loading mental engagement decks...");
        foreach (MentalChallengeDeckDTO dto in decks)
        {
            MentalChallengeDeck deck = dto.ToDomain();
            _gameWorld.MentalChallengeDecks[deck.Id] = deck;
            Console.WriteLine($"[PackageLoader] Loaded mental deck '{deck.Id}': {deck.Name} with {deck.CardIds.Count} cards");
        }
        Console.WriteLine($"[PackageLoader] Completed loading mental engagement decks. Total: {_gameWorld.MentalChallengeDecks.Count}");
    }


    private void LoadPhysicalChallengeDecks(List<PhysicalChallengeDeckDTO> decks, bool allowSkeletons)
    {
        if (decks == null) return;

        Console.WriteLine($"[PackageLoader] Loading physical engagement decks...");
        foreach (PhysicalChallengeDeckDTO dto in decks)
        {
            PhysicalChallengeDeck deck = dto.ToDomain();
            _gameWorld.PhysicalChallengeDecks[deck.Id] = deck;
            Console.WriteLine($"[PackageLoader] Loaded physical deck '{deck.Id}': {deck.Name} with {deck.CardIds.Count} cards");
        }
        Console.WriteLine($"[PackageLoader] Completed loading physical engagement decks. Total: {_gameWorld.PhysicalChallengeDecks.Count}");
    }

    private void LoadInvestigations(List<InvestigationDTO> investigations, bool allowSkeletons)
    {
        if (investigations == null) return;

        Console.WriteLine($"[PackageLoader] Loading investigation templates...");
        InvestigationParser parser = new InvestigationParser(_gameWorld);
        foreach (InvestigationDTO dto in investigations)
        {
            Investigation investigation = parser.ParseInvestigation(dto);
            _gameWorld.Investigations.Add(investigation);
            Console.WriteLine($"[PackageLoader] Loaded investigation '{investigation.Id}': {investigation.Name} ({investigation.PhaseDefinitions.Count} phases)");
        }
        Console.WriteLine($"[PackageLoader] Completed loading investigations. Total: {_gameWorld.Investigations.Count}");
    }

    private void LoadKnowledge(List<KnowledgeDTO> knowledgeList, bool allowSkeletons)
    {
        if (knowledgeList == null) return;

        Console.WriteLine($"[PackageLoader] Loading knowledge definitions...");
        foreach (KnowledgeDTO dto in knowledgeList)
        {
            Knowledge knowledge = KnowledgeParser.ParseKnowledge(dto);
            _gameWorld.Knowledge.Add(knowledge.Id, knowledge);
            Console.WriteLine($"[PackageLoader] Loaded knowledge '{knowledge.Id}': {knowledge.DisplayName}");
        }
        Console.WriteLine($"[PackageLoader] Completed loading knowledge. Total: {_gameWorld.Knowledge.Count}");
    }

    private void LoadGoals(List<GoalDTO> goalDtos, bool allowSkeletons)
    {
        if (goalDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {goalDtos.Count} goals...");

        foreach (GoalDTO dto in goalDtos)
        {
            // Parse goal using GoalParser
            Goal goal = GoalParser.ConvertDTOToGoal(dto, _gameWorld);

            // Add to centralized GameWorld.Goals storage
            _gameWorld.Goals[goal.Id] = goal;
            Console.WriteLine($"[PackageLoader] Parsed goal '{goal.Id}': {goal.Name} ({goal.SystemType})");

            // Assign goal to NPC or Location based on PlacementNpcId/PlacementLocationId
            if (!string.IsNullOrEmpty(goal.PlacementNpcId))
            {
                // Social goal - assign to NPC.ActiveGoals
                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);
                if (npc != null)
                {
                    if (npc.ActiveGoals == null)
                        npc.ActiveGoals = new List<Goal>();

                    npc.ActiveGoals.Add(goal);
                    Console.WriteLine($"[PackageLoader] Assigned Social goal '{goal.Name}' to NPC '{npc.Name}'");
                }
                else
                {
                    Console.WriteLine($"[PackageLoader] WARNING: Goal '{goal.Id}' references NPC '{goal.PlacementNpcId}' which doesn't exist yet");
                }
            }
            else if (!string.IsNullOrEmpty(goal.PlacementLocationId))
            {
                // Mental/Physical goal - assign to Location.ActiveGoals
                Location location = _gameWorld.GetLocation(goal.PlacementLocationId);
                if (location != null)
                {
                    if (location.ActiveGoals == null)
                        location.ActiveGoals = new List<Goal>();

                    location.ActiveGoals.Add(goal);
                    Console.WriteLine($"[PackageLoader] Assigned {goal.SystemType} goal '{goal.Name}' to location '{location.Name}'");
                }
                else
                {
                    Console.WriteLine($"[PackageLoader] WARNING: Goal '{goal.Id}' references location '{goal.PlacementLocationId}' which doesn't exist yet");
                }
            }
        }

        Console.WriteLine($"[PackageLoader] Completed loading goals. Total: {_gameWorld.Goals.Count}");
    }

    private void LoadObstacles(List<ObstacleDTO> obstacleDtos, bool allowSkeletons)
    {
        if (obstacleDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {obstacleDtos.Count} obstacles from package '{_currentPackageId}'...");

        foreach (ObstacleDTO dto in obstacleDtos)
        {
            // Parse obstacle using ObstacleParser
            Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(dto, _currentPackageId, _gameWorld);

            // Duplicate ID protection - prevent data corruption
            if (!_gameWorld.Obstacles.Any(o => o.Id == obstacle.Id))
            {
                _gameWorld.Obstacles.Add(obstacle);
                Console.WriteLine($"[PackageLoader] Loaded obstacle '{obstacle.Id}': {obstacle.Name} ({obstacle.Goals.Count} goals)");
            }
            else
            {
                throw new InvalidOperationException(
                    $"Duplicate obstacle ID '{obstacle.Id}' found in package '{_currentPackageId}'. " +
                    $"Obstacle IDs must be globally unique across all packages.");
            }
        }

        Console.WriteLine($"[PackageLoader] Completed loading obstacles. Total: {_gameWorld.Obstacles.Count}");
    }

    private void LoadLocations(List<VenueDTO> venueDtos, bool allowSkeletons)
    {
        if (venueDtos == null) return;

        foreach (VenueDTO dto in venueDtos)
        {
            // Check if this venue was previously a skeleton, if so replace it
            Venue? existingSkeleton = _gameWorld.WorldState.venues
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);

            Venue venue = VenueParser.ConvertDTOToVenue(dto);

            if (existingSkeleton != null)
            {
                Console.WriteLine($"[PackageLoader] Replacing skeleton venue {dto.Id}");

                _gameWorld.WorldState.venues.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }
            }

            _gameWorld.WorldState.venues.Add(venue);
        }
    }

    private void LoadLocationSpots(List<LocationDTO> spotDtos, bool allowSkeletons)
    {
        if (spotDtos == null) return;

        foreach (LocationDTO dto in spotDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            Location? existingSkeleton = _gameWorld.GetLocation(dto.Id);

            if (existingSkeleton != null && existingSkeleton.IsSkeleton)
            {
                SkeletonRegistryEntry? spotSkeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (spotSkeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(spotSkeletonEntry);
                }

                // Remove from primary Locations dictionary if exists
                _gameWorld.Locations.RemoveSpot(dto.Id);
            }

            Location location = LocationParser.ConvertDTOToLocation(dto, _gameWorld);

            // Add to primary Locations dictionary
            _gameWorld.Locations.AddOrUpdateSpot(location.Id, location);
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos, bool allowSkeletons)
    {
        if (npcDtos == null) return;

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if NPC references a Venue that doesn't exist
            if (!string.IsNullOrEmpty(dto.VenueId) &&
                !_gameWorld.WorldState.venues.Any(l => l.Id == dto.VenueId))
            {
                // Create skeleton venue
                Venue skeletonVenue = SkeletonGenerator.GenerateSkeletonVenue(
                    dto.VenueId,
                    $"npc_{dto.Id}_reference");

                _gameWorld.WorldState.venues.Add(skeletonVenue);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.VenueId, "Venue");

                // Also create a skeleton location for the location
                string hubSpotId = $"{dto.VenueId}_hub";
                Location hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    hubSpotId,
                    dto.VenueId,
                    $"location_{dto.VenueId}_hub");

                _gameWorld.Locations.AddOrUpdateSpot(hubSpotId, hubSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(hubSpot.Id, "Location");
            }

            // Check if NPC references a location that doesn't exist
            if (!string.IsNullOrEmpty(dto.LocationId) &&
                _gameWorld.GetLocation(dto.LocationId) == null)
            {
                // Create skeleton location
                Location skeletonSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.LocationId,
                    dto.VenueId ?? "unknown_location",
                    $"npc_{dto.Id}_spot_reference");

                _gameWorld.Locations.AddOrUpdateSpot(dto.LocationId, skeletonSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.LocationId, "Location");
            }

            // Check if this NPC was previously a skeleton, if so replace it and preserve persistent decks
            NPC? existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {
                Console.WriteLine($"[PackageLoader] Replacing skeleton NPC '{existingSkeleton.Name}' (ID: {existingSkeleton.ID}) with real content");

                // Preserve all cards from the persistent decks
                List<ExchangeCard> preservedExchangeCards = existingSkeleton.ExchangeDeck?.ToList() ?? new List<ExchangeCard>();

                int totalPreservedCards = preservedExchangeCards.Count;

                Console.WriteLine($"[PackageLoader] Preserving {totalPreservedCards} cards from persistent decks:");
                Console.WriteLine($"  - Exchange: {preservedExchangeCards.Count} cards");

                // Remove skeleton from game world
                _gameWorld.NPCs.Remove(existingSkeleton);
                _gameWorld.WorldState.NPCs.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }

                // Create new NPC from DTO
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld);

                // Restore preserved cards to the new NPC's persistent decks
                if (preservedExchangeCards.Any())
                {
                    npc.ExchangeDeck.AddRange(preservedExchangeCards);
                }

                Console.WriteLine($"[PackageLoader] Successfully replaced skeleton with real NPC and preserved all persistent deck cards");

                // Add the new NPC to game world
                _gameWorld.NPCs.Add(npc);
                _gameWorld.WorldState.NPCs.Add(npc);
            }
            else
            {
                // No skeleton to replace, just create new NPC normally
                NPC npc = NPCParser.ConvertDTOToNPC(dto, _gameWorld);
                _gameWorld.NPCs.Add(npc);
                _gameWorld.WorldState.NPCs.Add(npc);
            }
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos, bool allowSkeletons)
    {
        if (routeDtos == null)
        {
            Console.WriteLine("[PackageLoader] No routes to load - routeDtos is null");
            return;
        }

        Console.WriteLine($"[PackageLoader] Loading {routeDtos.Count} routes...");
        Console.WriteLine($"[PackageLoader] Currently loaded Locations: {string.Join(", ", _gameWorld.Locations.Select(s => s.LocationId))}");

        // Check for missing Locations and handle based on allowSkeletons
        foreach (RouteDTO dto in routeDtos)
        {
            // Check origin location
            Location originSpot = _gameWorld.GetLocation(dto.OriginSpotId);
            if (originSpot == null)
            {
                if (allowSkeletons)
                {
                    Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing origin location '{dto.OriginSpotId}' - creating skeleton");

                    // Create skeleton location with crossroads property (required for routes)
                    originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.OriginSpotId,
                        dto.OriginVenueId ?? "unknown_location",
                        $"route_{dto.Id}_origin"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!originSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        originSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.Locations.AddOrUpdateSpot(dto.OriginSpotId, originSpot);
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.OriginSpotId, "Location");

                    Console.WriteLine($"[PackageLoader] Created skeleton location '{dto.OriginSpotId}' for route '{dto.Id}'");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing origin location '{dto.OriginSpotId}'. Ensure Locations are loaded before routes.");
                }
            }

            // Check destination location
            Location destSpot = _gameWorld.GetLocation(dto.DestinationSpotId);
            if (destSpot == null)
            {
                if (allowSkeletons)
                {
                    Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing destination location '{dto.DestinationSpotId}' - creating skeleton");

                    // Create skeleton location with crossroads property (required for routes)
                    destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.DestinationSpotId,
                        dto.DestinationVenueId ?? "unknown_location",
                        $"route_{dto.Id}_destination"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!destSpot.LocationProperties.Contains(LocationPropertyType.Crossroads))
                    {
                        destSpot.LocationProperties.Add(LocationPropertyType.Crossroads);
                    }

                    _gameWorld.Locations.AddOrUpdateSpot(dto.DestinationSpotId, destSpot);
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.DestinationSpotId, "Location");

                    Console.WriteLine($"[PackageLoader] Created skeleton location '{dto.DestinationSpotId}' for route '{dto.Id}'");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing destination location '{dto.DestinationSpotId}'. Ensure Locations are loaded before routes.");
                }
            }
        }

        // BIDIRECTIONAL ROUTE PRINCIPLE: Routes are defined once in JSON but automatically
        // generate both directions. This eliminates redundancy and ensures consistency.
        // The return journey has segments in reversed order (A->B->C becomes C->B->A).
        foreach (RouteDTO dto in routeDtos)
        {
            // Create the forward route from JSON
            RouteOption forwardRoute = ConvertRouteDTOToModel(dto);
            _gameWorld.WorldState.Routes.Add(forwardRoute);
            Console.WriteLine($"[PackageLoader] Added route {forwardRoute.Id}: {forwardRoute.OriginLocationSpot} -> {forwardRoute.DestinationLocationSpot}");

            // Automatically generate the reverse route
            RouteOption reverseRoute = GenerateReverseRoute(forwardRoute);
            _gameWorld.WorldState.Routes.Add(reverseRoute);
            Console.WriteLine($"[PackageLoader] Generated reverse route {reverseRoute.Id}: {reverseRoute.OriginLocationSpot} -> {reverseRoute.DestinationLocationSpot}");
        }

        Console.WriteLine($"[PackageLoader] Completed loading {routeDtos.Count} routes. Total routes with bidirectional: {_gameWorld.WorldState.Routes.Count}");
    }

    private string GetVenueIdFromSpotId(string LocationId)
    {
        Location? location = _gameWorld.GetLocation(LocationId);
        if (location == null)
        {
            Console.WriteLine($"[PackageLoader] GetVenueIdFromSpotId: location '{LocationId}' not found");
            Console.WriteLine($"[PackageLoader] Available Locations: {string.Join(", ", _gameWorld.Locations.Select(s => s.LocationId))}");
        }
        else if (string.IsNullOrEmpty(location.VenueId))
        {
            Console.WriteLine($"[PackageLoader] GetVenueIdFromSpotId: location '{LocationId}' found but has no VenueId set");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] GetVenueIdFromSpotId: Found location '{LocationId}' with VenueId '{location.VenueId}'");
        }
        return location?.VenueId;
    }

    private void LoadDialogueTemplates(DialogueTemplates dialogueTemplates, bool allowSkeletons)
    {
        if (dialogueTemplates == null) return;

        Console.WriteLine($"[PackageLoader] Loading dialogue templates...");
        _gameWorld.DialogueTemplates = dialogueTemplates;
        Console.WriteLine($"[PackageLoader] Dialogue templates loaded successfully");
    }

    private List<PathCardEntry> LoadPathCards(List<PathCardDTO> pathCardDtos, bool allowSkeletons)
    {
        if (pathCardDtos == null) return new List<PathCardEntry>();

        Console.WriteLine($"[PackageLoader] Loading {pathCardDtos.Count} path cards...");

        List<PathCardEntry> pathCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in pathCardDtos)
        {
            pathCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
            Console.WriteLine($"[PackageLoader] Loaded path card '{dto.Id}': {dto.Name}");
        }

        Console.WriteLine($"[PackageLoader] Completed loading path cards. Total: {pathCardLookup.Count}");
        return pathCardLookup;
    }

    private List<PathCardEntry> LoadEventCards(List<PathCardDTO> eventCardDtos, bool allowSkeletons)
    {
        if (eventCardDtos == null) return new List<PathCardEntry>();

        Console.WriteLine($"[PackageLoader] Loading {eventCardDtos.Count} event cards...");

        List<PathCardEntry> eventCardLookup = new List<PathCardEntry>();
        foreach (PathCardDTO dto in eventCardDtos)
        {
            eventCardLookup.Add(new PathCardEntry { Id = dto.Id, Card = dto });
            Console.WriteLine($"[PackageLoader] Loaded event card '{dto.Id}': {dto.Name}");
        }

        Console.WriteLine($"[PackageLoader] Completed loading event cards. Total: {eventCardLookup.Count}");
        return eventCardLookup;
    }

    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, List<PathCardEntry> eventCardLookup, bool allowSkeletons)
    {
        if (travelEventDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {travelEventDtos.Count} travel events...");

        foreach (TravelEventDTO dto in travelEventDtos)
        {
            // Embed actual event cards if this event has event card IDs (for JSON loaded events)
            if (dto.EventCardIds != null && dto.EventCards.Count == 0)
            {
                foreach (string cardId in dto.EventCardIds)
                {
                    PathCardEntry? eventCardEntry = eventCardLookup.FirstOrDefault(e => e.Id == cardId);
                    if (eventCardEntry != null)
                    {
                        dto.EventCards.Add(eventCardEntry.Card);
                    }
                }
            }

            _gameWorld.AllTravelEvents.Add(new TravelEventEntry { EventId = dto.Id, TravelEvent = dto });
            Console.WriteLine($"[PackageLoader] Loaded travel event '{dto.Id}': {dto.Name} with {dto.EventCards.Count} event cards");
        }

        Console.WriteLine($"[PackageLoader] Completed loading travel events. Total: {_gameWorld.AllTravelEvents.Count}");
    }


    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, List<PathCardEntry> pathCardLookup, List<PathCardEntry> eventCardLookup, bool allowSkeletons)
    {
        if (collectionDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {collectionDtos.Count} collections and embedding cards directly...");

        foreach (PathCardCollectionDTO dto in collectionDtos)
        {
            // Embed actual path cards if this collection has path card IDs (for JSON loaded collections)
            if (dto.PathCards != null && dto.PathCards.Count == 0 && dto.PathCardIds != null)
            {
                foreach (string cardId in dto.PathCardIds)
                {
                    PathCardEntry? pathCardEntry = pathCardLookup.FirstOrDefault(p => p.Id == cardId);
                    if (pathCardEntry != null)
                    {
                        dto.PathCards.Add(pathCardEntry.Card);
                    }
                }
            }

            // Determine if this is a path collection or event collection based on contents
            bool isEventCollection = (dto.Events != null && dto.Events.Count > 0);
            bool isPathCollection = (dto.PathCards != null && dto.PathCards.Count > 0);

            if (isEventCollection)
            {
                // This is an event collection - contains child events for random selection
                _gameWorld.AllEventCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
                Console.WriteLine($"[PackageLoader] Loaded event collection '{dto.Id}': {dto.Name} with {dto.Events.Count} events");
            }
            else if (isPathCollection)
            {
                // This is a path collection - contains actual path cards for FixedPath segments
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
                Console.WriteLine($"[PackageLoader] Loaded path collection '{dto.Id}': {dto.Name} with {dto.PathCards.Count} path cards");
            }
            else
            {
                // Fallback: treat as path collection if no clear indicators
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { CollectionId = dto.Id, Collection = dto });
                Console.WriteLine($"[PackageLoader] Loaded collection '{dto.Id}' as path collection (default): {dto.Name}");
            }
        }

        Console.WriteLine($"[PackageLoader] Completed loading collections. Path collections: {_gameWorld.AllPathCollections.Count}, Event collections: {_gameWorld.AllEventCollections.Count}");
    }



    /// <summary>
    /// Initialize exchange decks for Mercantile NPCs only
    /// </summary>
    private void InitializeNPCExchangeDecks(DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC exchange decks...");

        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                List<ExchangeCard> npcExchangeCards = new List<ExchangeCard>();

                // Check deck compositions for this NPC's exchange deck
                NPCDeckDefinitionDTO deckDef = null;
                if (deckCompositions != null)
                {
                    // Check for NPC-specific deck first
                    if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                    {
                        deckDef = deckCompositions.NpcDecks[npc.ID];
                        Console.WriteLine($"[PackageLoader] Using custom exchange deck for {npc.Name}");
                    }
                    // No default deck anymore - NPCs only have specific decks
                }

                // Build exchange deck from composition
                if (deckDef != null && deckDef.ExchangeDeck != null)
                {
                    foreach (KeyValuePair<string, int> kvp in deckDef.ExchangeDeck)
                    {
                        string cardId = kvp.Key;
                        int count = kvp.Value;

                        // Find the exchange card from the parsed exchange cards
                        ExchangeCardEntry? exchangeEntry = _parsedExchangeCards?.FirstOrDefault(e => e.Id == cardId);
                        if (exchangeEntry != null)
                        {
                            ExchangeCard exchangeCard = exchangeEntry.Card;
                            // Add the specified number of copies to the deck
                            for (int i = 0; i < count; i++)
                            {
                                // Exchange cards are templates, no need to clone
                                npcExchangeCards.Add(exchangeCard);
                                Console.WriteLine($"[PackageLoader] Added exchange card {cardId} to {npc.Name}'s deck");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PackageLoader] WARNING: Exchange card {cardId} not found in parsed exchanges for {npc.Name}");
                        }
                    }
                }
                else if (npc.PersonalityType == PersonalityType.MERCANTILE)
                {
                    // Create default exchanges for mercantile NPCs without specific exchanges
                    npcExchangeCards = ExchangeParser.CreateDefaultExchangesForNPC(npc);
                    if (npcExchangeCards.Count > 0)
                    {
                        Console.WriteLine($"[PackageLoader] Created {npcExchangeCards.Count} default exchange cards for {npc.Name}");
                    }
                }

                // Initialize exchange deck
                npc.InitializeExchangeDeck(npcExchangeCards);

                if (npcExchangeCards.Count > 0)
                {
                    Console.WriteLine($"[PackageLoader] Initialized exchange deck for {npc.Name} with {npcExchangeCards.Count} cards");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"[PackageLoader] Failed to initialize exchange deck for NPC {npc.Name}: {ex.Message}", ex);
            }
        }

        Console.WriteLine("[PackageLoader] NPC exchange deck initialization completed");
    }

    private void LoadItems(List<ItemDTO> itemDtos, bool allowSkeletons)
    {
        if (itemDtos == null) return;

        foreach (ItemDTO dto in itemDtos)
        {
            Item item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.WorldState.Items.Add(item);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos, bool allowSkeletons)
    {
        if (obligationDtos == null) return;

        foreach (StandingObligationDTO dto in obligationDtos)
        {
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto);
            _gameWorld.WorldState.StandingObligationTemplates.Add(obligation);
        }
    }

    private void LoadLocationActions(List<VenueActionDTO> locationActionDtos, bool allowSkeletons)
    {
        if (locationActionDtos == null) return;

        foreach (VenueActionDTO dto in locationActionDtos)
        {
            LocationAction locationAction = ConvertLocationActionDTOToModel(dto);
            _gameWorld.LocationActions.Add(locationAction);
        }
    }

    // Conversion methods that don't have dedicated parsers yet


    private RouteOption ConvertRouteDTOToModel(RouteDTO dto)
    {
        RouteOption route = new RouteOption
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            OriginLocationSpot = dto.OriginSpotId ?? "",
            DestinationLocationSpot = dto.DestinationSpotId ?? "",
            Method = Enum.TryParse<TravelMethods>(dto.Method, out TravelMethods method) ? method : TravelMethods.Walking,
            BaseCoinCost = dto.BaseCoinCost,
            BaseStaminaCost = dto.BaseStaminaCost,
            TravelTimeSegments = dto.TravelTimeSegments,
            IsDiscovered = dto.IsDiscovered,
            Description = dto.Description ?? "",
            MaxItemCapacity = dto.MaxItemCapacity > 0 ? dto.MaxItemCapacity : 3
        };


        // Parse terrain categories
        if (dto.TerrainCategories != null)
        {
            foreach (string category in dto.TerrainCategories)
            {
                if (Enum.TryParse<TerrainCategory>(category, out TerrainCategory terrain))
                {
                    route.TerrainCategories.Add(terrain);
                }
            }
        }

        // Parse travel path cards system properties
        route.StartingStamina = dto.StartingStamina;

        // Parse route segments
        if (dto.Segments != null)
        {
            foreach (RouteSegmentDTO segmentDto in dto.Segments)
            {
                // Parse segment type
                SegmentType segmentType = SegmentType.FixedPath; // Default
                if (!string.IsNullOrEmpty(segmentDto.Type))
                {
                    Enum.TryParse<SegmentType>(segmentDto.Type, out segmentType);
                }

                RouteSegment segment = new RouteSegment
                {
                    SegmentNumber = segmentDto.SegmentNumber,
                    Type = segmentType
                };

                // Set collection properties based on segment type using normalized properties
                if (segmentType == SegmentType.FixedPath)
                {
                    // FixedPath segments use pathCollectionId from JSON
                    segment.PathCollectionId = segmentDto.PathCollectionId;
                    if (!string.IsNullOrEmpty(segment.PathCollectionId))
                    {
                        Console.WriteLine($"[PackageLoader] FixedPath segment {segmentDto.SegmentNumber} uses path collection '{segment.PathCollectionId}'");
                    }
                }
                else if (segmentType == SegmentType.Event)
                {
                    // Event segments use eventCollectionId from JSON
                    segment.EventCollectionId = segmentDto.EventCollectionId;
                    if (!string.IsNullOrEmpty(segment.EventCollectionId))
                    {
                        Console.WriteLine($"[PackageLoader] Event segment {segmentDto.SegmentNumber} uses event collection '{segment.EventCollectionId}'");
                    }
                }

                route.Segments.Add(segment);
            }
        }

        // Parse encounter deck IDs
        if (dto.EncounterDeckIds != null)
        {
            route.EncounterDeckIds.AddRange(dto.EncounterDeckIds);
        }

        // Parse obstacles on this route (bandits, flooding, difficult terrain)
        if (dto.Obstacles != null && dto.Obstacles.Count > 0)
        {
            foreach (ObstacleDTO obstacleDto in dto.Obstacles)
            {
                Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(obstacleDto, route.Id, _gameWorld);
                route.Obstacles.Add(obstacle);
            }
            Console.WriteLine($"[PackageLoader] Parsed {route.Obstacles.Count} obstacles for route '{route.Name}'");
        }

        return route;
    }

    private ObservationCard ConvertObservationDTOToCard(ObservationDTO dto)
    {
        ObservationCard observationCard = new()
        {
            Id = dto.Id,
            Title = dto.DisplayText ?? "",
            InitiativeCost = dto.InitiativeCost,
            TokenType = ConnectionType.Trust,
            Persistence = PersistenceType.Statement,
            SuccessType = SuccessEffectType.None,
            DialogueText = "",
        };

        return observationCard;
    }

    private Observation ConvertDTOToObservation(ObservationDTO dto)
    {
        // Parse observation type from category
        ObservationType observationType = ObservationType.Normal;
        if (dto.Category != null && Enum.TryParse<ObservationType>(dto.Category, out ObservationType parsedType))
        {
            observationType = parsedType;
        }

        return new Observation
        {
            Id = dto.Id ?? "",
            Text = dto.DisplayText ?? dto.Description ?? dto.Name ?? "",
            Type = observationType,
            AttentionCost = 0, // From DTO or default to 0
            RelevantNPCs = new string[0], // Empty by default - could be populated from properties
            CreatesState = ConnectionState.NEUTRAL, // No state creation from DTO
            CardTemplate = dto.Id ?? "", // Use ID as template
            Description = dto.Description ?? dto.DisplayText ?? "",
            ProvidesInfo = ObservationInfoType.Location, // Could be derived from properties
            CreatesUrgency = false, // Default false
            Automatic = false, // Default false
            LocationId = dto.LocationId // Will be set by location-specific loading if needed
        };
    }

    private LocationAction ConvertLocationActionDTOToModel(VenueActionDTO dto)
    {
        LocationAction action = new LocationAction
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            Cost = dto.Cost ?? new Dictionary<string, int>(),
            Reward = dto.Reward ?? new Dictionary<string, int>(),
            TimeRequired = dto.TimeRequired,
            Availability = dto.Availability ?? new List<string>(),
            Priority = dto.Priority,
            ActionType = dto.ActionType ?? "",
            InvestigationId = dto.InvestigationId
        };

        // Parse required properties
        if (dto.RequiredProperties != null)
        {
            foreach (string prop in dto.RequiredProperties)
            {
                if (Enum.TryParse<LocationPropertyType>(prop, true, out LocationPropertyType propertyType))
                {
                    action.RequiredProperties.Add(propertyType);
                }
            }
        }

        // Parse optional properties
        if (dto.OptionalProperties != null)
        {
            foreach (string prop in dto.OptionalProperties)
            {
                if (Enum.TryParse<LocationPropertyType>(prop, true, out LocationPropertyType propertyType))
                {
                    action.OptionalProperties.Add(propertyType);
                }
            }
        }

        // Parse excluded properties
        if (dto.ExcludedProperties != null)
        {
            foreach (string prop in dto.ExcludedProperties)
            {
                if (Enum.TryParse<LocationPropertyType>(prop, true, out LocationPropertyType propertyType))
                {
                    action.ExcludedProperties.Add(propertyType);
                }
            }
        }

        return action;
    }

    private void LoadExchanges(List<ExchangeDTO> exchangeDtos, bool allowSkeletons)
    {
        if (exchangeDtos == null) return;

        // Store DTOs for reference
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            _gameWorld.ExchangeDefinitions.Add(dto);
            Console.WriteLine($"[PackageLoader] Loaded exchange definition: {dto.Id} ({dto.GiveAmount} {dto.GiveCurrency} for {dto.ReceiveAmount} {dto.ReceiveCurrency})");
        }

        // Parse exchanges into ExchangeCard objects and store them
        // These will be referenced when building NPC decks
        _parsedExchangeCards = new List<ExchangeCardEntry>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto, dto.NpcId);
            _parsedExchangeCards.Add(new ExchangeCardEntry { Id = exchangeCard.Id, Card = exchangeCard });
            Console.WriteLine($"[PackageLoader] Created ExchangeCard: {exchangeCard.Id} - {exchangeCard.GetExchangeRatio()}");
        }
    }

    /// <summary>
    /// Initialize the travel discovery system state after all content is loaded
    /// </summary>
    private void InitializeTravelDiscoverySystem()
    {
        Console.WriteLine("[PackageLoader] Initializing travel discovery system...");

        // Initialize PathCardDiscoveries from cards embedded in collections
        // First from path collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllPathCollections.GetAllCollections())
        {
            foreach (PathCardDTO pathCard in collection.PathCards)
            {
                _gameWorld.PathCardDiscoveries.SetDiscovered(pathCard.Id, pathCard.StartsRevealed);
                Console.WriteLine($"[PackageLoader] Path card '{pathCard.Id}' discovery state: {(pathCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }

        // Also initialize discovery states for event cards in event collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllEventCollections.GetAllCollections())
        {
            foreach (PathCardDTO eventCard in collection.EventCards)
            {
                _gameWorld.PathCardDiscoveries.SetDiscovered(eventCard.Id, eventCard.StartsRevealed);
                Console.WriteLine($"[PackageLoader] Event card '{eventCard.Id}' discovery state: {(eventCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }

        // Cards are now embedded directly in collections
        // No separate card dictionaries needed

        // Initialize EventDeckPositions for routes with event pools
        foreach (PathCollectionEntry entry in _gameWorld.AllEventCollections)
        {
            string routeId = entry.CollectionId;
            string deckKey = $"route_{routeId}_events";

            // Start at position 0 for deterministic event drawing
            _gameWorld.EventDeckPositions.SetPosition(deckKey, 0);

            int eventCount = entry.Collection.Events?.Count ?? 0;
            Console.WriteLine($"[PackageLoader] Initialized event deck position for route '{routeId}' with {eventCount} events");
        }

        // Initialize route discovery states
        // Routes can start discovered based on IsDiscovered property
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            if (route.IsDiscovered)
            {
                Console.WriteLine($"[PackageLoader] Route '{route.Id}' starts discovered");
            }
            else
            {
                Console.WriteLine($"[PackageLoader] Route '{route.Id}' starts hidden, needs discovery");
            }
        }

        Console.WriteLine($"[PackageLoader] Travel discovery system initialized: {_gameWorld.PathCardDiscoveries.Count} path cards, {_gameWorld.EventDeckPositions.Count} event decks");
    }

    /// <summary>
    /// Initialize investigation journal with all investigations as Potential (awaiting discovery triggers)
    /// Called AFTER all packages are loaded to ensure all investigations exist
    /// </summary>
    private void InitializeInvestigationJournal()
    {
        Console.WriteLine("[PackageLoader] Initializing investigation journal...");

        InvestigationJournal investigationJournal = _gameWorld.InvestigationJournal;

        investigationJournal.PotentialInvestigationIds.Clear();
        foreach (Investigation investigation in _gameWorld.Investigations)
        {
            investigationJournal.PotentialInvestigationIds.Add(investigation.Id);
            Console.WriteLine($"[PackageLoader] Added investigation to journal: {investigation.Id} ({investigation.Name})");
        }
        Console.WriteLine($"[PackageLoader] Investigation journal initialized with {investigationJournal.PotentialInvestigationIds.Count} potential investigations");
    }

    /// <summary>
    /// Validates that crossroads configuration is correct:
    /// 1. Each Venue has exactly one location with Crossroads property
    /// 2. All route origin and destination Locations have Crossroads property
    /// </summary>
    /// <summary>
    /// BIDIRECTIONAL ROUTE GENERATION: Automatically creates the reverse route from a forward route.
    /// This ensures travel is always bidirectional and segments are properly reversed.
    /// For example, a route A->B->C with segments [1,2,3] becomes C->B->A with segments [3,2,1].
    /// </summary>
    private RouteOption GenerateReverseRoute(RouteOption forwardRoute)
    {
        // Extract Venue IDs from the location IDs for naming
        string originVenueId = GetVenueIdFromSpotId(forwardRoute.OriginLocationSpot);
        string destVenueId = GetVenueIdFromSpotId(forwardRoute.DestinationLocationSpot);

        // Generate reverse route ID by swapping origin and destination
        string[] idParts = forwardRoute.Id.Split("_to_");
        string reverseId = idParts.Length == 2
            ? $"{idParts[1]}_to_{idParts[0]}"
            : $"{destVenueId}_to_{originVenueId}";

        RouteOption reverseRoute = new RouteOption
        {
            Id = reverseId,
            Name = $"Return to {GetLocationNameFromId(originVenueId)}",
            // Swap origin and destination
            OriginLocationSpot = forwardRoute.DestinationLocationSpot,
            DestinationLocationSpot = forwardRoute.OriginLocationSpot,

            // Keep the same properties for both directions
            Method = forwardRoute.Method,
            BaseCoinCost = forwardRoute.BaseCoinCost,
            BaseStaminaCost = forwardRoute.BaseStaminaCost,
            TravelTimeSegments = forwardRoute.TravelTimeSegments,
            DepartureTime = forwardRoute.DepartureTime,
            IsDiscovered = forwardRoute.IsDiscovered,
            MaxItemCapacity = forwardRoute.MaxItemCapacity,
            Description = $"Return journey from {GetLocationNameFromId(destVenueId)} to {GetLocationNameFromId(originVenueId)}",
            AccessRequirement = forwardRoute.AccessRequirement,
            RouteType = forwardRoute.RouteType,
            HasPermitUnlock = forwardRoute.HasPermitUnlock,
            StartingStamina = forwardRoute.StartingStamina
        };

        // Copy terrain categories
        reverseRoute.TerrainCategories.AddRange(forwardRoute.TerrainCategories);

        // Copy weather modifications
        foreach (KeyValuePair<WeatherCondition, RouteModification> kvp in forwardRoute.WeatherModifications)
        {
            reverseRoute.WeatherModifications[kvp.Key] = kvp.Value;
        }

        // Copy unlock condition if present
        if (forwardRoute.UnlockCondition != null)
        {
            reverseRoute.UnlockCondition = forwardRoute.UnlockCondition;
        }

        // CRITICAL: Reverse the segments order for the return journey
        // This ensures the path is traversed in reverse (C->B->A instead of A->B->C)
        List<RouteSegment> reversedSegments = forwardRoute.Segments.OrderByDescending(s => s.SegmentNumber).ToList();
        int segmentNumber = 1;
        foreach (RouteSegment? originalSegment in reversedSegments)
        {
            RouteSegment reverseSegment = new RouteSegment
            {
                SegmentNumber = segmentNumber++,
                Type = originalSegment.Type,
                // Keep the same collections - they represent the same physical locations
                PathCollectionId = originalSegment.PathCollectionId,
                EventCollectionId = originalSegment.EventCollectionId
            };
            reverseRoute.Segments.Add(reverseSegment);
        }

        // Copy encounter deck IDs
        reverseRoute.EncounterDeckIds.AddRange(forwardRoute.EncounterDeckIds);

        // If the forward route has a route-level event pool, copy it to the reverse route
        PathCollectionEntry? forwardEntry = _gameWorld.AllEventCollections.FindById(forwardRoute.Id);
        if (forwardEntry != null)
        {
            _gameWorld.AllEventCollections.AddOrUpdateCollection(reverseRoute.Id, forwardEntry.Collection);
        }

        return reverseRoute;
    }

    private string GetLocationNameFromId(string venueId)
    {
        // Helper to get friendly Venue name from ID for route naming
        if (string.IsNullOrEmpty(venueId))
        {
            return "Unknown Location";
        }

        Venue? venue = _gameWorld.WorldState.venues?.FirstOrDefault(l => l.Id == venueId);
        return venue?.Name ?? venueId.Replace("_", " ").Replace("-", " ");
    }

    private void ValidateCrossroadsConfiguration()
    {
        Console.WriteLine("[PackageLoader] Starting crossroads configuration validation...");

        // Group Locations by Venue using tuples
        List<(string VenueId, List<Location> Locations)> spotsByLocation = new List<(string VenueId, List<Location> Locations)>();
        foreach (LocationEntry entry in _gameWorld.Locations)
        {
            Location location = entry.location;
            int groupIndex = spotsByLocation.FindIndex(g => g.VenueId == location.VenueId);
            if (groupIndex == -1)
            {
                List<Location> Locations = new List<Location>();
                Locations.Add(location);
                spotsByLocation.Add((location.VenueId, Locations));
            }
            else
            {
                spotsByLocation[groupIndex].Locations.Add(location);
            }
        }

        // Validate each venue has exactly one crossroads location
        foreach (Venue venue in _gameWorld.WorldState.venues)
        {
            (string VenueId, List<Location> Locations) locationGroup = spotsByLocation.FirstOrDefault(g => g.VenueId == venue.Id);
            if (locationGroup.VenueId == null)
            {
                throw new InvalidOperationException($"Venue '{venue.Id}' ({venue.Name}) has no Locations defined");
            }

            List<Location> locations = locationGroup.Locations;
            List<Location> crossroadsSpots = locations
                .Where(s => s.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
                .ToList();

            if (crossroadsSpots.Count == 0)
            {
                throw new InvalidOperationException($"Location '{venue.Id}' ({venue.Name}) has no Locations with Crossroads property. Every Venue must have exactly one crossroads location for travel.");
            }
            else if (crossroadsSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", crossroadsSpots.Select(s => $"'{s.Id}' ({s.Name})"));
                throw new InvalidOperationException($"Location '{venue.Id}' ({venue.Name}) has {crossroadsSpots.Count} Locations with Crossroads property: {spotsInfo}. Only one crossroads location is allowed per location.");
            }

            Console.WriteLine($"[PackageLoader] Venue '{venue.Id}' has valid crossroads location: '{crossroadsSpots[0].Id}'");
        }

        // Validate all route Locations have crossroads property
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocationSpot))
                routeSpotIds.Add(route.OriginLocationSpot);
            if (!routeSpotIds.Contains(route.DestinationLocationSpot))
                routeSpotIds.Add(route.DestinationLocationSpot);
        }

        foreach (string LocationId in routeSpotIds)
        {
            Location location = _gameWorld.GetLocation(LocationId);
            if (location == null)
            {
                Console.WriteLine($"[PackageLoader] Route references missing location '{LocationId}' - creating skeleton");

                // Create skeleton location with crossroads property (required for routes)
                location = SkeletonGenerator.GenerateSkeletonSpot(
                    LocationId,
                    "unknown_location",
                    $"crossroads_validation_{LocationId}"
                );

                // Ensure skeleton has crossroads property for route connectivity
                if (!location.LocationProperties.Contains(LocationPropertyType.Crossroads))
                {
                    location.LocationProperties.Add(LocationPropertyType.Crossroads);
                }

                _gameWorld.Locations.AddOrUpdateSpot(LocationId, location);
                _gameWorld.SkeletonRegistry.AddSkeleton(LocationId, "Location");

                Console.WriteLine($"[PackageLoader] Created skeleton location '{LocationId}' with Crossroads property for route validation");
            }

            if (!location.LocationProperties?.Contains(LocationPropertyType.Crossroads) == true)
            {
                Console.WriteLine($"[PackageLoader] Route location '{LocationId}' ({location.Name}) missing Crossroads property - adding it");
                location.LocationProperties.Add(LocationPropertyType.Crossroads);
            }

            Console.WriteLine($"[PackageLoader] Route location '{LocationId}' has valid Crossroads property");
        }

        Console.WriteLine($"[PackageLoader] Crossroads validation completed successfully. Validated {_gameWorld.WorldState.venues.Count} venues and {routeSpotIds.Count} route Locations.");
    }


    private void LoadTravelObstacles(List<TravelObstacleDTO> obstacleDtos, bool allowSkeletons)
    {
        if (obstacleDtos == null) return;

        Console.WriteLine($"[PackageLoader] Loading {obstacleDtos.Count} travel obstacles...");

        TravelObstacleParser parser = new TravelObstacleParser();
        foreach (TravelObstacleDTO dto in obstacleDtos)
        {
            TravelObstacle obstacle = parser.ParseTravelObstacle(dto);
            _gameWorld.TravelObstacles.Add(obstacle);
            Console.WriteLine($"[PackageLoader] Loaded travel obstacle '{obstacle.Id}': {obstacle.Name} ({obstacle.Type}) with {obstacle.Approaches.Count} approaches");
        }

        Console.WriteLine($"[PackageLoader] Completed loading travel obstacles. Total: {_gameWorld.TravelObstacles.Count}");
    }

}