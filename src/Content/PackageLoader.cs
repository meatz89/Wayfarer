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

        // 2. Locations and spots (may reference regions/districts)
        LoadLocations(package.Content.Locations, allowSkeletons);
        LoadLocationSpots(package.Content.Spots, allowSkeletons);

        // 3. Cards (foundation for NPCs and conversations)
        LoadCards(package.Content.Cards, allowSkeletons);
        LoadConversationTypesAndDecks(package.Content.ConversationTypes, package.Content.CardDecks, allowSkeletons);
        LoadNpcRequestCards(package.Content.NpcRequestCards, allowSkeletons);
        LoadPromiseCards(package.Content.PromiseCards, allowSkeletons);

        // 4. NPCs (reference locations, spots, and cards)
        LoadNPCs(package.Content.Npcs, allowSkeletons);
        LoadStrangers(package.Content.Strangers, allowSkeletons);

        // 5. Routes (reference spots which now have LocationId set)
        LoadRoutes(package.Content.Routes, allowSkeletons);

        // 6. Relationship entities (depend on NPCs and cards)
        LoadExchanges(package.Content.Exchanges, allowSkeletons);
        InitializeNPCRequests(package.Content.NpcRequests, package.Content.NpcGoalCards, package.Content.DeckCompositions);
        InitializeNPCExchangeDecks(package.Content.DeckCompositions);

        // 7. Complex entities
        LoadObservations(package.Content.Observations, allowSkeletons);
        LoadDialogueTemplates(package.Content.DialogueTemplates, allowSkeletons);
        LoadInvestigationRewards(package.Content.InvestigationRewards, allowSkeletons);
        LoadLetterTemplates(package.Content.LetterTemplates, allowSkeletons);
        LoadStandingObligations(package.Content.StandingObligations, allowSkeletons);
        LoadLocationActions(package.Content.LocationActions, allowSkeletons);

        // 8. Travel content
        List<PathCardEntry> pathCardLookup = LoadPathCards(package.Content.PathCards, allowSkeletons);
        List<PathCardEntry> eventCardLookup = LoadEventCards(package.Content.EventCards, allowSkeletons);
        LoadTravelEvents(package.Content.TravelEvents, eventCardLookup, allowSkeletons);
        LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup, allowSkeletons);
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
    public List<string> LoadDynamicPackageFromJson(string json, string packageId = null)
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
                tokenEntry.Commerce = kvp.Value.Commerce;
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

            Console.WriteLine($"[PackageLoader] Added stranger '{stranger.Name}' (Level {stranger.Level}) to location '{stranger.Location}' for time block '{stranger.AvailableTimeBlock}'");
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
                LocationIds = dto.LocationIds ?? new List<string>(),
                DistrictType = dto.DistrictType,
                DangerLevel = dto.DangerLevel,
                Characteristics = dto.Characteristics ?? new List<string>()
            };
            _gameWorld.WorldState.Districts.Add(district);
        }
    }

    private void LoadCards(List<ConversationCardDTO> cardDtos, bool allowSkeletons)
    {
        if (cardDtos == null) return;

        foreach (ConversationCardDTO dto in cardDtos)
        {
            // Use static method from ConversationCardParser
            ConversationCard card = ConversationCardParser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions.AddOrUpdateCard(card.Id, card);
        }
    }

    private void LoadConversationTypesAndDecks(List<ConversationTypeDefinitionDTO> conversationTypeDtos, List<CardDeckDTO> cardDeckDtos, bool allowSkeletons)
    {
        // Load card decks first (conversation types reference them)
        if (cardDeckDtos != null)
        {
            Console.WriteLine($"[PackageLoader] Loading {cardDeckDtos.Count} card decks...");
            foreach (CardDeckDTO dto in cardDeckDtos)
            {
                // Convert cardCounts to cardIds list
                List<string> cardIds = new List<string>();
                foreach (KeyValuePair<string, int> kvp in dto.CardCounts)
                {
                    for (int i = 0; i < kvp.Value; i++)
                    {
                        cardIds.Add(kvp.Key);
                    }
                }

                CardDeckDefinition deck = new CardDeckDefinition
                {
                    Id = dto.Id,
                    CardIds = cardIds
                };
                _gameWorld.CardDecks.AddOrUpdateDeck(deck.Id, deck);
                Console.WriteLine($"[PackageLoader] Loaded card deck '{deck.Id}' with {deck.CardIds.Count} cards");
            }
        }

        // Load conversation types (they reference card decks by ID)
        if (conversationTypeDtos != null)
        {
            Console.WriteLine($"[PackageLoader] Loading {conversationTypeDtos.Count} conversation types...");
            foreach (ConversationTypeDefinitionDTO dto in conversationTypeDtos)
            {
                ConversationTypeDefinition conversationType = new ConversationTypeDefinition
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    DeckId = dto.DeckId,
                    Category = dto.Category,
                    AvailableTimeBlocks = dto.AvailableTimeBlocks ?? new List<string>(),
                    DoubtPerListen = dto.DoubtPerListen ?? 0,
                    MomentumErosion = dto.MomentumErosion ?? false,
                    MaxDoubt = dto.MaxDoubt ?? 10
                };
                _gameWorld.ConversationTypes.AddOrUpdateConversationType(conversationType.Id, conversationType);
                Console.WriteLine($"[PackageLoader] Loaded conversation type '{conversationType.Id}' using deck '{conversationType.DeckId}'");
            }
        }
    }

    private void LoadNpcRequestCards(Dictionary<string, List<ConversationCardDTO>> npcRequestCards, bool allowSkeletons)
    {
        if (npcRequestCards == null) return;

        Console.WriteLine($"[PackageLoader] Loading NPC request cards...");
        foreach (KeyValuePair<string, List<ConversationCardDTO>> kvp in npcRequestCards)
        {
            string npcId = kvp.Key;
            foreach (ConversationCardDTO dto in kvp.Value)
            {
                ConversationCard card = ConversationCardParser.ConvertDTOToCard(dto);
                _gameWorld.AllCardDefinitions.AddOrUpdateCard(card.Id, card);
                Console.WriteLine($"[PackageLoader] Loaded request card '{card.Id}' for NPC '{npcId}'");
            }
        }
    }


    private void LoadPromiseCards(List<ConversationCardDTO> promiseCards, bool allowSkeletons)
    {
        if (promiseCards == null) return;

        Console.WriteLine($"[PackageLoader] Loading promise cards...");
        foreach (ConversationCardDTO dto in promiseCards)
        {
            ConversationCard card = ConversationCardParser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions.AddOrUpdateCard(card.Id, card);
            Console.WriteLine($"[PackageLoader] Loaded promise card '{card.Id}'");
        }
    }


    private void LoadLocations(List<LocationDTO> locationDtos, bool allowSkeletons)
    {
        if (locationDtos == null) return;

        foreach (LocationDTO dto in locationDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            Location? existingSkeleton = _gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);

            Location location = LocationParser.ConvertDTOToLocation(dto);

            if (existingSkeleton != null)
            {
                // Preserve player's familiarity with skeleton location
                int preservedFamiliarity = existingSkeleton.Familiarity;

                // Transfer familiarity to real location (capped by MaxFamiliarity)
                location.Familiarity = Math.Min(preservedFamiliarity, location.MaxFamiliarity);

                Console.WriteLine($"[PackageLoader] Preserving familiarity for location {dto.Id}: {preservedFamiliarity} -> {location.Familiarity} (max: {location.MaxFamiliarity})");

                _gameWorld.WorldState.locations.Remove(existingSkeleton);
                _gameWorld.Locations.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }
            }

            _gameWorld.Locations.Add(location);
            _gameWorld.WorldState.locations.Add(location);
        }
    }

    private void LoadLocationSpots(List<LocationSpotDTO> spotDtos, bool allowSkeletons)
    {
        if (spotDtos == null) return;

        foreach (LocationSpotDTO dto in spotDtos)
        {
            // Check if this spot was previously a skeleton, if so replace it
            LocationSpot? existingSkeleton = _gameWorld.WorldState.locationSpots
                .FirstOrDefault(s => s.SpotID == dto.Id && s.IsSkeleton);

            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locationSpots.Remove(existingSkeleton);
                SkeletonRegistryEntry? spotSkeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (spotSkeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(spotSkeletonEntry);
                }

                // Remove from primary spots dictionary if exists
                _gameWorld.Spots.RemoveSpot(dto.Id);
            }

            LocationSpot spot = LocationSpotParser.ConvertDTOToLocationSpot(dto);
            _gameWorld.WorldState.locationSpots.Add(spot);

            // Add to primary spots dictionary
            _gameWorld.Spots.AddOrUpdateSpot(spot.SpotID, spot);
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos, bool allowSkeletons)
    {
        if (npcDtos == null) return;

        foreach (NPCDTO dto in npcDtos)
        {
            // Check if NPC references a location that doesn't exist
            if (!string.IsNullOrEmpty(dto.LocationId) &&
                !_gameWorld.WorldState.locations.Any(l => l.Id == dto.LocationId))
            {
                // Create skeleton location
                Location skeletonLocation = SkeletonGenerator.GenerateSkeletonLocation(
                    dto.LocationId,
                    $"npc_{dto.Id}_reference");

                _gameWorld.WorldState.locations.Add(skeletonLocation);
                _gameWorld.Locations.Add(skeletonLocation);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.LocationId, "Location");

                // Also create a skeleton spot for the location
                string hubSpotId = $"{dto.LocationId}_hub";
                LocationSpot hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    hubSpotId,
                    dto.LocationId,
                    $"location_{dto.LocationId}_hub");

                _gameWorld.WorldState.locationSpots.Add(hubSpot);
                _gameWorld.Spots.AddOrUpdateSpot(hubSpotId, hubSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(hubSpot.SpotID, "LocationSpot");
            }

            // Check if NPC references a spot that doesn't exist
            if (!string.IsNullOrEmpty(dto.SpotId) &&
                !_gameWorld.WorldState.locationSpots.Any(s => s.SpotID == dto.SpotId))
            {
                // Create skeleton spot
                LocationSpot skeletonSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    dto.SpotId,
                    dto.LocationId ?? "unknown_location",
                    $"npc_{dto.Id}_spot_reference");

                _gameWorld.WorldState.locationSpots.Add(skeletonSpot);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.SpotId, "LocationSpot");
            }

            // Check if this NPC was previously a skeleton, if so replace it and preserve persistent decks
            NPC? existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {
                Console.WriteLine($"[PackageLoader] Replacing skeleton NPC '{existingSkeleton.Name}' (ID: {existingSkeleton.ID}) with real content");

                // Preserve all cards from the persistent decks
                List<ExchangeCard> preservedExchangeCards = existingSkeleton.ExchangeDeck?.ToList() ?? new List<ExchangeCard>();
                List<ConversationCard> preservedObservationCards = existingSkeleton.ObservationDeck?.GetAllCards()?.ToList() ?? new List<ConversationCard>();
                List<ConversationCard> preservedBurdenCards = existingSkeleton.BurdenDeck?.GetAllCards()?.ToList() ?? new List<ConversationCard>();
                List<NPCRequest> preservedRequests = existingSkeleton.Requests?.ToList() ?? new List<NPCRequest>();

                int totalPreservedCards = preservedExchangeCards.Count +
                                        preservedObservationCards.Count + preservedBurdenCards.Count +
                                        preservedRequests.SelectMany(r => r.PromiseCardIds).Count();

                Console.WriteLine($"[PackageLoader] Preserving {totalPreservedCards} cards from persistent decks:");
                Console.WriteLine($"  - Exchange: {preservedExchangeCards.Count} cards");
                Console.WriteLine($"  - Observation: {preservedObservationCards.Count} cards");
                Console.WriteLine($"  - Burden: {preservedBurdenCards.Count} cards");
                Console.WriteLine($"  - Requests: {preservedRequests.Count} request objects");

                // Remove skeleton from game world
                _gameWorld.NPCs.Remove(existingSkeleton);
                _gameWorld.WorldState.NPCs.Remove(existingSkeleton);
                SkeletonRegistryEntry? skeletonEntry = _gameWorld.SkeletonRegistry.FindById(dto.Id);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }

                // Create new NPC from DTO
                NPC npc = NPCParser.ConvertDTOToNPC(dto);

                // Restore preserved cards to the new NPC's persistent decks
                if (preservedExchangeCards.Any())
                {
                    npc.ExchangeDeck.AddRange(preservedExchangeCards);
                }

                if (preservedObservationCards.Any())
                {
                    foreach (ConversationCard? card in preservedObservationCards)
                    {
                        npc.ObservationDeck.AddCard(card);
                    }
                }

                if (preservedBurdenCards.Any())
                {
                    foreach (ConversationCard? card in preservedBurdenCards)
                    {
                        npc.BurdenDeck.AddCard(card);
                    }
                }

                if (preservedRequests.Any())
                {
                    // Merge preserved requests with new NPC's requests
                    // Note: This assumes no ID conflicts between preserved and new requests
                    npc.Requests.AddRange(preservedRequests);
                }

                Console.WriteLine($"[PackageLoader] Successfully replaced skeleton with real NPC and preserved all persistent deck cards");

                // Add the new NPC to game world
                _gameWorld.NPCs.Add(npc);
                _gameWorld.WorldState.NPCs.Add(npc);
            }
            else
            {
                // No skeleton to replace, just create new NPC normally
                NPC npc = NPCParser.ConvertDTOToNPC(dto);
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
        Console.WriteLine($"[PackageLoader] Currently loaded spots: {string.Join(", ", _gameWorld.Spots.Select(s => s.SpotId))}");

        // Check for missing spots and handle based on allowSkeletons
        foreach (RouteDTO dto in routeDtos)
        {
            // Check origin spot
            LocationSpot originSpot = _gameWorld.GetSpot(dto.OriginSpotId);
            if (originSpot == null)
            {
                if (allowSkeletons)
                {
                    Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing origin spot '{dto.OriginSpotId}' - creating skeleton");

                    // Create skeleton spot with crossroads property (required for routes)
                    originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.OriginSpotId,
                        dto.OriginLocationId ?? "unknown_location",
                        $"route_{dto.Id}_origin"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!originSpot.SpotProperties.Contains(SpotPropertyType.Crossroads))
                    {
                        originSpot.SpotProperties.Add(SpotPropertyType.Crossroads);
                    }

                    _gameWorld.WorldState.locationSpots.Add(originSpot);
                    _gameWorld.Spots.Add(new LocationSpotEntry { SpotId = dto.OriginSpotId, Spot = originSpot });
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.OriginSpotId, "LocationSpot");

                    Console.WriteLine($"[PackageLoader] Created skeleton spot '{dto.OriginSpotId}' for route '{dto.Id}'");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing origin spot '{dto.OriginSpotId}'. Ensure spots are loaded before routes.");
                }
            }

            // Check destination spot
            LocationSpot destSpot = _gameWorld.GetSpot(dto.DestinationSpotId);
            if (destSpot == null)
            {
                if (allowSkeletons)
                {
                    Console.WriteLine($"[PackageLoader] Route '{dto.Id}' references missing destination spot '{dto.DestinationSpotId}' - creating skeleton");

                    // Create skeleton spot with crossroads property (required for routes)
                    destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.DestinationSpotId,
                        dto.DestinationLocationId ?? "unknown_location",
                        $"route_{dto.Id}_destination"
                    );

                    // Ensure skeleton has crossroads property for route connectivity
                    if (!destSpot.SpotProperties.Contains(SpotPropertyType.Crossroads))
                    {
                        destSpot.SpotProperties.Add(SpotPropertyType.Crossroads);
                    }

                    _gameWorld.WorldState.locationSpots.Add(destSpot);
                    _gameWorld.Spots.Add(new LocationSpotEntry { SpotId = dto.DestinationSpotId, Spot = destSpot });
                    _gameWorld.SkeletonRegistry.AddSkeleton(dto.DestinationSpotId, "LocationSpot");

                    Console.WriteLine($"[PackageLoader] Created skeleton spot '{dto.DestinationSpotId}' for route '{dto.Id}'");
                }
                else
                {
                    throw new Exception($"[PackageLoader] Route '{dto.Id}' references missing destination spot '{dto.DestinationSpotId}'. Ensure spots are loaded before routes.");
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
            AddRouteToLocationConnections(forwardRoute);

            // Automatically generate the reverse route
            RouteOption reverseRoute = GenerateReverseRoute(forwardRoute);
            _gameWorld.WorldState.Routes.Add(reverseRoute);
            Console.WriteLine($"[PackageLoader] Generated reverse route {reverseRoute.Id}: {reverseRoute.OriginLocationSpot} -> {reverseRoute.DestinationLocationSpot}");
            AddRouteToLocationConnections(reverseRoute);
        }

        Console.WriteLine($"[PackageLoader] Completed loading {routeDtos.Count} routes. Total routes with bidirectional: {_gameWorld.WorldState.Routes.Count}");
    }

    private void AddRouteToLocationConnections(RouteOption route)
    {
        Console.WriteLine($"[PackageLoader] Adding route {route.Id} to location connections...");

        // Find the origin location for this route
        string originLocationId = GetLocationIdFromSpotId(route.OriginLocationSpot);
        string destinationLocationId = GetLocationIdFromSpotId(route.DestinationLocationSpot);

        Console.WriteLine($"[PackageLoader] Route {route.Id}: Origin spot '{route.OriginLocationSpot}' -> Location '{originLocationId}', Destination spot '{route.DestinationLocationSpot}' -> Location '{destinationLocationId}'");

        if (originLocationId == null || destinationLocationId == null)
        {
            throw new Exception($"[PackageLoader] Route {route.Id} has spots without LocationId. Origin: {route.OriginLocationSpot} -> {originLocationId}, Destination: {route.DestinationLocationSpot} -> {destinationLocationId}");
        }

        // Find the origin location
        Location? originLocation = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == originLocationId);
        if (originLocation == null)
        {
            Console.WriteLine($"[PackageLoader] Warning: Could not find origin location {originLocationId} for route {route.Id}");
            return;
        }

        // Find or create a connection to the destination location
        LocationConnection? connection = originLocation.Connections.FirstOrDefault(c => c.DestinationLocationId == destinationLocationId);
        if (connection == null)
        {
            connection = new LocationConnection
            {
                DestinationLocationId = destinationLocationId,
                RouteOptions = new List<RouteOption>()
            };
            originLocation.Connections.Add(connection);
            Console.WriteLine($"[PackageLoader] Created new connection from {originLocationId} to {destinationLocationId}");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] Found existing connection from {originLocationId} to {destinationLocationId}");
        }

        // Add the route to the connection
        if (!connection.RouteOptions.Any(r => r.Id == route.Id))
        {
            connection.RouteOptions.Add(route);
            Console.WriteLine($"[PackageLoader] Added route {route.Id} to connection. Connection now has {connection.RouteOptions.Count} routes");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] Route {route.Id} already exists in connection");
        }
    }

    private string GetLocationIdFromSpotId(string spotId)
    {
        LocationSpot? spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
        if (spot == null)
        {
            Console.WriteLine($"[PackageLoader] GetLocationIdFromSpotId: Spot '{spotId}' not found in WorldState.locationSpots");
            Console.WriteLine($"[PackageLoader] Available spots: {string.Join(", ", _gameWorld.WorldState.locationSpots.Select(s => s.SpotID))}");
        }
        else if (string.IsNullOrEmpty(spot.LocationId))
        {
            Console.WriteLine($"[PackageLoader] GetLocationIdFromSpotId: Spot '{spotId}' found but has no LocationId set");
        }
        else
        {
            Console.WriteLine($"[PackageLoader] GetLocationIdFromSpotId: Found spot '{spotId}' with LocationId '{spot.LocationId}'");
        }
        return spot?.LocationId;
    }

    private void LoadObservations(List<ObservationDTO> observationDtos, bool allowSkeletons)
    {
        if (observationDtos == null) return;

        foreach (ObservationDTO dto in observationDtos)
        {
            // Convert to ConversationCard for the existing system
            ConversationCard observation = ConvertObservationDTOToCard(dto);
            _gameWorld.PlayerObservationCards.Add(observation);

            // Also create Observation domain object and store in GameWorld
            Observation observationDomain = ConvertDTOToObservation(dto);
            _gameWorld.Observations.Add(observationDomain);
        }
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
    /// Initialize one-time requests for NPCs from NpcGoalCards and deck compositions
    /// </summary>
    private void InitializeNPCRequests(List<NPCRequestDTO> npcRequestDtos, List<NPCGoalCardDTO> npcGoalCardDtos, DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine($"[PackageLoader] Initializing NPC one-time requests... npcRequestDtos={npcRequestDtos?.Count ?? 0}, npcGoalCardDtos={npcGoalCardDtos?.Count ?? 0}");
        List<string> validationErrors = new List<string>();

        // First load NPCRequest bundles from npcRequests section
        if (npcRequestDtos != null && npcRequestDtos.Count > 0)
        {
            Console.WriteLine($"[PackageLoader] Processing {npcRequestDtos.Count} NPC requests from npcRequests section...");
            foreach (NPCRequestDTO requestDto in npcRequestDtos)
            {
                try
                {
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == requestDto.NpcId);
                    if (npc == null)
                    {
                        // Check if this is a skeleton NPC
                        if (!_gameWorld.SkeletonRegistry.Any(s => s.SkeletonKey == requestDto.NpcId))
                        {
                            throw PackageLoadException.CreateMissingDependency(
                                _currentPackageId,
                                "NPCRequest",
                                requestDto.Id,
                                $"NPC '{requestDto.NpcId}'",
                                $"Ensure NPC '{requestDto.NpcId}' is defined in the npcs section of this or a dependency package");
                        }
                        continue; // Skip skeleton NPCs for now
                    }

                    NPCRequest request = new NPCRequest
                    {
                        Id = requestDto.Id,
                        Name = requestDto.Name,
                        Description = requestDto.Description,
                        NpcRequestText = requestDto.NpcRequestText,
                        Status = RequestStatus.Available
                    };

                    // Validate request card IDs - these are REQUIRED for the request to function
                    if (requestDto.RequestCards != null)
                    {
                        foreach (string cardId in requestDto.RequestCards)
                        {
                            if (_gameWorld.AllCardDefinitions.Any(c => c.CardId == cardId))
                            {
                                request.RequestCardIds.Add(cardId);
                                Console.WriteLine($"[PackageLoader] Added request card ID '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                // Request cards are critical - log warning for now
                                Console.WriteLine($"[PackageLoader] Warning: Request card '{cardId}' not found in GameWorld.AllCardDefinitions");
                                // throw PackageLoadException.CreateMissingDependency(
                                //     _currentPackageId,
                                //     "NPCRequest",
                                //     requestDto.Id,
                                //     $"request card '{cardId}'",
                                //     $"Ensure card '{cardId}' is defined with type 'Request' in the cards section");
                            }
                        }
                    }

                    // Validate promise card IDs - these are also REQUIRED if specified
                    if (requestDto.PromiseCards != null)
                    {
                        foreach (string cardId in requestDto.PromiseCards)
                        {
                            if (_gameWorld.AllCardDefinitions.Any(c => c.CardId == cardId))
                            {
                                request.PromiseCardIds.Add(cardId);
                                Console.WriteLine($"[PackageLoader] Added promise card ID '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                // Promise cards are critical - throw exception
                                throw PackageLoadException.CreateMissingDependency(
                                    _currentPackageId,
                                    "NPCRequest",
                                    requestDto.Id,
                                    $"promise card '{cardId}'",
                                    $"Ensure card '{cardId}' is defined with type 'Promise' in the cards section");
                            }
                        }
                    }

                    // Only add the request if it has at least one card
                    if (request.RequestCardIds.Count > 0 || request.PromiseCardIds.Count > 0)
                    {
                        npc.Requests.Add(request);
                        Console.WriteLine($"[PackageLoader] Added request '{requestDto.Id}' to NPC '{npc.Name}' with {request.RequestCardIds.Count} request card IDs and {request.PromiseCardIds.Count} promise card IDs");
                    }
                    else
                    {
                        validationErrors.Add($"Request '{requestDto.Id}' has no request or promise cards defined");
                    }
                }
                catch (PackageLoadException)
                {
                    throw; // Re-throw PackageLoadException as-is
                }
                catch (Exception ex)
                {
                    throw new Exception($"[PackageLoader] Failed to load request '{requestDto.Id}': {ex.Message}", ex);
                }
            }

            // If we collected any validation errors, throw them all at once
            if (validationErrors.Count > 0)
            {
                throw PackageLoadException.MultipleValidationErrors(_currentPackageId, validationErrors);
            }

            Console.WriteLine($"[PackageLoader] Loaded {npcRequestDtos.Count} NPC request bundles");
        }
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
                                // Use the DeepClone method to create a copy
                                ExchangeCard cardCopy = exchangeCard.DeepClone();
                                npcExchangeCards.Add(cardCopy);
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

    private void LoadLetterTemplates(List<LetterTemplateDTO> letterDtos, bool allowSkeletons)
    {
        if (letterDtos == null) return;

        foreach (LetterTemplateDTO dto in letterDtos)
        {
            LetterTemplate letter = LetterTemplateParser.ConvertDTOToLetterTemplate(dto);
            _gameWorld.WorldState.LetterTemplates.Add(letter);
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

    private void LoadLocationActions(List<LocationActionDTO> locationActionDtos, bool allowSkeletons)
    {
        if (locationActionDtos == null) return;

        foreach (LocationActionDTO dto in locationActionDtos)
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

        return route;
    }

    private ConversationCard ConvertObservationDTOToCard(ObservationDTO dto)
    {
        // Observations become player cards
        return new ConversationCard
        {
            Id = dto.Id,
            Description = dto.DisplayText ?? "",
            InitiativeCost = dto.InitiativeCost,
            TokenType = ConnectionType.Trust,
            Difficulty = Difficulty.Medium,
            CardType = CardType.Observation,
            Persistence = PersistenceType.Statement, // Observations persist through LISTEN
            SuccessType = SuccessEffectType.None,
            FailureType = FailureEffectType.None,
            PersonalityTypes = new List<string>(),
            LevelBonuses = new List<CardLevelBonus>(),
            DialogueFragment = "",
            VerbPhrase = ""
        };
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
            CreatesState = null, // No state creation from DTO
            CardTemplate = dto.Id ?? "", // Use ID as template
            Description = dto.Description ?? dto.DisplayText ?? "",
            ProvidesInfo = null, // Could be derived from properties
            CreatesUrgency = false, // Default false
            Automatic = false, // Default false
            SpotId = null // Will be set by location-specific loading if needed
        };
    }

    private LocationAction ConvertLocationActionDTOToModel(LocationActionDTO dto)
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
            Icon = dto.Icon ?? "[WORK]",
            Priority = dto.Priority,
            ActionType = dto.ActionType ?? ""
        };

        // Parse required properties
        if (dto.RequiredProperties != null)
        {
            foreach (string prop in dto.RequiredProperties)
            {
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
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
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
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
                if (Enum.TryParse<SpotPropertyType>(prop, true, out SpotPropertyType propertyType))
                {
                    action.ExcludedProperties.Add(propertyType);
                }
            }
        }

        return action;
    }

    private void LoadInvestigationRewards(List<ObservationRewardDTO> investigationRewardDtos, bool allowSkeletons)
    {
        if (investigationRewardDtos == null) return;

        foreach (ObservationRewardDTO dto in investigationRewardDtos)
        {
            // Find the target location
            Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
            if (location != null)
            {
                // Convert DTO to domain model
                ObservationReward reward = ConvertObservationRewardDTOToModel(dto);
                location.ObservationRewards.Add(reward);
            }
            else
            {
                // Create skeleton location if not found
                Location skeletonLocation = SkeletonGenerator.GenerateSkeletonLocation(dto.LocationId, $"investigation_reward_{dto.LocationId}");
                ObservationReward reward = ConvertObservationRewardDTOToModel(dto);
                skeletonLocation.ObservationRewards.Add(reward);
                _gameWorld.Locations.Add(skeletonLocation);
                _gameWorld.SkeletonRegistry.AddSkeleton(dto.LocationId, "Location");
            }
        }
    }

    private ObservationReward ConvertObservationRewardDTOToModel(ObservationRewardDTO dto)
    {
        return new ObservationReward
        {
            LocationId = dto.LocationId,
            FamiliarityRequired = dto.FamiliarityRequired,
            PriorObservationRequired = dto.PriorObservationRequired,
            ObservationCard = new ObservationCardReward
            {
                Id = dto.ObservationCard.Id,
                Name = dto.ObservationCard.Name,
                TargetNpcId = dto.ObservationCard.TargetNpcId,
                Effect = dto.ObservationCard.Effect,
                Description = dto.ObservationCard.Description
            }
        };
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
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto);
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
    /// Validates that crossroads configuration is correct:
    /// 1. Each location has exactly one spot with Crossroads property
    /// 2. All route origin and destination spots have Crossroads property
    /// </summary>
    /// <summary>
    /// BIDIRECTIONAL ROUTE GENERATION: Automatically creates the reverse route from a forward route.
    /// This ensures travel is always bidirectional and segments are properly reversed.
    /// For example, a route A->B->C with segments [1,2,3] becomes C->B->A with segments [3,2,1].
    /// </summary>
    private RouteOption GenerateReverseRoute(RouteOption forwardRoute)
    {
        // Extract location IDs from the spot IDs for naming
        string originLocationId = GetLocationIdFromSpotId(forwardRoute.OriginLocationSpot);
        string destLocationId = GetLocationIdFromSpotId(forwardRoute.DestinationLocationSpot);

        // Generate reverse route ID by swapping origin and destination
        string[] idParts = forwardRoute.Id.Split("_to_");
        string reverseId = idParts.Length == 2
            ? $"{idParts[1]}_to_{idParts[0]}"
            : $"{destLocationId}_to_{originLocationId}";

        RouteOption reverseRoute = new RouteOption
        {
            Id = reverseId,
            Name = $"Return to {GetLocationNameFromId(originLocationId)}",
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
            Description = $"Return journey from {GetLocationNameFromId(destLocationId)} to {GetLocationNameFromId(originLocationId)}",
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

    private string GetLocationNameFromId(string locationId)
    {
        // Helper to get friendly location name from ID for route naming
        if (string.IsNullOrEmpty(locationId))
        {
            return "Unknown Location";
        }

        Location? location = _gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == locationId);
        return location?.Name ?? locationId.Replace("_", " ").Replace("-", " ");
    }

    private void ValidateCrossroadsConfiguration()
    {
        Console.WriteLine("[PackageLoader] Starting crossroads configuration validation...");

        // Group spots by location using tuples
        List<(string LocationId, List<LocationSpot> Spots)> spotsByLocation = new List<(string LocationId, List<LocationSpot> Spots)>();
        foreach (LocationSpot spot in _gameWorld.WorldState.locationSpots)
        {
            int groupIndex = spotsByLocation.FindIndex(g => g.LocationId == spot.LocationId);
            if (groupIndex == -1)
            {
                List<LocationSpot> spots = new List<LocationSpot>();
                spots.Add(spot);
                spotsByLocation.Add((spot.LocationId, spots));
            }
            else
            {
                spotsByLocation[groupIndex].Spots.Add(spot);
            }
        }

        // Validate each location has exactly one crossroads spot
        foreach (Location location in _gameWorld.WorldState.locations)
        {
            (string LocationId, List<LocationSpot> Spots) locationGroup = spotsByLocation.FirstOrDefault(g => g.LocationId == location.Id);
            if (locationGroup.LocationId == null)
            {
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has no spots defined");
            }

            List<LocationSpot> locationSpots = locationGroup.Spots;
            List<LocationSpot> crossroadsSpots = locationSpots
                .Where(s => s.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true)
                .ToList();

            if (crossroadsSpots.Count == 0)
            {
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has no spots with Crossroads property. Every location must have exactly one crossroads spot for travel.");
            }
            else if (crossroadsSpots.Count > 1)
            {
                string spotsInfo = string.Join(", ", crossroadsSpots.Select(s => $"'{s.SpotID}' ({s.Name})"));
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has {crossroadsSpots.Count} spots with Crossroads property: {spotsInfo}. Only one crossroads spot is allowed per location.");
            }

            Console.WriteLine($"[PackageLoader] Location '{location.Id}' has valid crossroads spot: '{crossroadsSpots[0].SpotID}'");
        }

        // Validate all route spots have crossroads property
        List<string> routeSpotIds = new List<string>();
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            if (!routeSpotIds.Contains(route.OriginLocationSpot))
                routeSpotIds.Add(route.OriginLocationSpot);
            if (!routeSpotIds.Contains(route.DestinationLocationSpot))
                routeSpotIds.Add(route.DestinationLocationSpot);
        }

        foreach (string spotId in routeSpotIds)
        {
            LocationSpot spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
            if (spot == null)
            {
                Console.WriteLine($"[PackageLoader] Route references missing spot '{spotId}' - creating skeleton");

                // Create skeleton spot with crossroads property (required for routes)
                spot = SkeletonGenerator.GenerateSkeletonSpot(
                    spotId,
                    "unknown_location",
                    $"crossroads_validation_{spotId}"
                );

                // Ensure skeleton has crossroads property for route connectivity
                if (!spot.SpotProperties.Contains(SpotPropertyType.Crossroads))
                {
                    spot.SpotProperties.Add(SpotPropertyType.Crossroads);
                }

                _gameWorld.WorldState.locationSpots.Add(spot);
                _gameWorld.Spots.AddOrUpdateSpot(spotId, spot);
                _gameWorld.SkeletonRegistry.AddSkeleton(spotId, "LocationSpot");

                Console.WriteLine($"[PackageLoader] Created skeleton spot '{spotId}' with Crossroads property for route validation");
            }

            if (!spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true)
            {
                Console.WriteLine($"[PackageLoader] Route spot '{spotId}' ({spot.Name}) missing Crossroads property - adding it");
                spot.SpotProperties.Add(SpotPropertyType.Crossroads);
            }

            Console.WriteLine($"[PackageLoader] Route spot '{spotId}' has valid Crossroads property");
        }

        Console.WriteLine($"[PackageLoader] Crossroads validation completed successfully. Validated {_gameWorld.WorldState.locations.Count} locations and {routeSpotIds.Count} route spots.");
    }

}