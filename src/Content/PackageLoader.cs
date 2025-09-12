using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Orchestrates loading of game packages, delegating to specialized parsers for conversion
/// </summary>
public class PackageLoader
{
    private readonly GameWorld _gameWorld;
    private bool _isFirstPackage = true;
    private Dictionary<string, ExchangeCard> _parsedExchangeCards;

    public PackageLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Load a single package from a JSON file
    /// </summary>
    public void LoadPackage(string packageFilePath)
    {
        string json = File.ReadAllText(packageFilePath);
        Package package = JsonSerializer.Deserialize<Package>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        LoadPackageContent(package);
    }

    /// <summary>
    /// Load multiple packages in sequence
    /// </summary>
    public void LoadPackages(List<string> packageFilePaths)
    {
        foreach (string path in packageFilePaths)
        {
            LoadPackage(path);
        }
    }

    /// <summary>
    /// Load all packages from a directory with proper ordering
    /// </summary>
    public void LoadPackagesFromDirectory(string directoryPath)
    {
        List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
            .OrderBy(f =>
            {
                // Core packages first (priority 0)
                if (f.Contains("core", StringComparison.OrdinalIgnoreCase)) return 0;
                // Base content next (priority 1)
                if (f.Contains("base", StringComparison.OrdinalIgnoreCase)) return 1;
                // Expansions (priority 2)
                if (f.Contains("expansion", StringComparison.OrdinalIgnoreCase)) return 2;
                // Generated content last (priority 3)
                if (f.Contains("generated", StringComparison.OrdinalIgnoreCase)) return 3;
                // Everything else in between
                return 2;
            })
            .ThenBy(f => f) // Then alphabetical
            .ToList();

        LoadPackages(packageFiles);
        
        // Validate crossroads configuration after all packages are loaded
        ValidateCrossroadsConfiguration();
    }

    private void LoadPackageContent(Package package)
    {
        // Apply starting conditions only from the first package
        if (_isFirstPackage && package.StartingConditions != null)
        {
            ApplyStartingConditions(package.StartingConditions);
            _isFirstPackage = false;
        }

        // Load content in dependency order
        if (package.Content != null)
        {
            // Phase 0: Geographic hierarchy (regions and districts)
            LoadRegions(package.Content.Regions);
            LoadDistricts(package.Content.Districts);

            // Phase 1: Cards (foundation - NPCs reference these)
            LoadCards(package.Content.Cards);

            // Phase 2: Locations (NPCs and spots reference these)
            LoadLocations(package.Content.Locations);

            // Phase 3: Location spots (depend on locations)
            LoadLocationSpots(package.Content.Spots);

            // Phase 4: NPCs (depend on locations and cards)
            LoadNPCs(package.Content.Npcs);

            // Phase 4.5: Load exchanges BEFORE initializing exchange decks
            LoadExchanges(package.Content.Exchanges);

            // Phase 4.6: Initialize all NPC decks (after NPCs, cards, and exchanges are loaded)
            InitializeNPCConversationDecks(package.Content.DeckCompositions);
            InitializeNPCRequests(package.Content.NpcRequests, package.Content.NpcGoalCards, package.Content.DeckCompositions);
            InitializeNPCExchangeDecks(package.Content.DeckCompositions);

            // Phase 5: Routes (depend on locations)
            LoadRoutes(package.Content.Routes);

            // Phase 6: Independent content
            LoadObservations(package.Content.Observations);
            LoadInvestigationRewards(package.Content.InvestigationRewards);
            // Load path cards first, then collections that reference them
            var pathCardLookup = LoadPathCards(package.Content.PathCards);
            
            // Load event system components (normalized structure)
            var eventCardLookup = LoadEventCards(package.Content.EventCards);
            LoadTravelEvents(package.Content.TravelEvents, eventCardLookup);
            
            // Load collections (handles both path collections and event collections)
            LoadEventCollections(package.Content.PathCardCollections, pathCardLookup, eventCardLookup);
            LoadItems(package.Content.Items);
            LoadLetterTemplates(package.Content.LetterTemplates);
            LoadStandingObligations(package.Content.StandingObligations);
            LoadLocationActions(package.Content.LocationActions);
            
            // Initialize travel discovery system after all content is loaded
            InitializeTravelDiscoverySystem();
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
                _gameWorld.GetPlayer().NPCTokens[kvp.Key] = new Dictionary<ConnectionType, int>
                {
                    [ConnectionType.Trust] = kvp.Value.Trust,
                    [ConnectionType.Commerce] = kvp.Value.Commerce,
                    [ConnectionType.Status] = kvp.Value.Status,
                    [ConnectionType.Shadow] = kvp.Value.Shadow
                };
            }
        }
    }

    private void LoadRegions(List<RegionDTO> regionDtos)
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

    private void LoadDistricts(List<DistrictDTO> districtDtos)
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

    private void LoadCards(List<ConversationCardDTO> cardDtos)
    {
        if (cardDtos == null) return;

        foreach (ConversationCardDTO dto in cardDtos)
        {
            // Use static method from ConversationCardParser
            ConversationCard card = ConversationCardParser.ConvertDTOToCard(dto);
            _gameWorld.AllCardDefinitions[card.Id] = card;
        }
    }

    private void LoadLocations(List<LocationDTO> locationDtos)
    {
        if (locationDtos == null) return;

        foreach (LocationDTO dto in locationDtos)
        {
            // Check if this location was previously a skeleton, if so replace it
            Location? existingSkeleton = _gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);

            if (existingSkeleton != null)
            {
                _gameWorld.WorldState.locations.Remove(existingSkeleton);
                _gameWorld.Locations.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
            }

            Location location = LocationParser.ConvertDTOToLocation(dto);
            _gameWorld.Locations.Add(location);
            _gameWorld.WorldState.locations.Add(location);
        }
    }

    private void LoadLocationSpots(List<LocationSpotDTO> spotDtos)
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
                _gameWorld.SkeletonRegistry.Remove(dto.Id);

                // Remove from primary spots dictionary if exists
                _gameWorld.Spots.Remove(dto.Id);
            }

            LocationSpot spot = LocationSpotParser.ConvertDTOToLocationSpot(dto);
            _gameWorld.WorldState.locationSpots.Add(spot);

            // Add to primary spots dictionary
            _gameWorld.Spots[spot.SpotID] = spot;
        }
    }

    private void LoadNPCs(List<NPCDTO> npcDtos)
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
                _gameWorld.SkeletonRegistry[dto.LocationId] = "Location";

                // Also create a skeleton spot for the location
                string hubSpotId = $"{dto.LocationId}_hub";
                LocationSpot hubSpot = SkeletonGenerator.GenerateSkeletonSpot(
                    hubSpotId,
                    dto.LocationId,
                    $"location_{dto.LocationId}_hub");

                _gameWorld.WorldState.locationSpots.Add(hubSpot);
                _gameWorld.Spots[hubSpotId] = hubSpot;
                _gameWorld.SkeletonRegistry[hubSpot.SpotID] = "LocationSpot";
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
                _gameWorld.SkeletonRegistry[dto.SpotId] = "LocationSpot";
            }

            // Check if this NPC was previously a skeleton, if so replace it
            NPC? existingSkeleton = _gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
            if (existingSkeleton != null)
            {
                _gameWorld.NPCs.Remove(existingSkeleton);
                _gameWorld.WorldState.NPCs.Remove(existingSkeleton);
                _gameWorld.SkeletonRegistry.Remove(dto.Id);
            }

            NPC npc = NPCParser.ConvertDTOToNPC(dto);
            _gameWorld.NPCs.Add(npc);
            _gameWorld.WorldState.NPCs.Add(npc);
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos)
    {
        if (routeDtos == null)
        {
            Console.WriteLine("[PackageLoader] No routes to load - routeDtos is null");
            return;
        }

        Console.WriteLine($"[PackageLoader] Loading {routeDtos.Count} routes...");

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
            Console.WriteLine($"[PackageLoader] Warning: Could not find location IDs for route {route.Id}. Origin: {route.OriginLocationSpot}, Destination: {route.DestinationLocationSpot}");
            return;
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
        return spot?.LocationId;
    }

    private void LoadObservations(List<ObservationDTO> observationDtos)
    {
        if (observationDtos == null) return;

        foreach (ObservationDTO dto in observationDtos)
        {
            ConversationCard observation = ConvertObservationDTOToCard(dto);
            _gameWorld.PlayerObservationCards.Add(observation);
        }
    }

    private Dictionary<string, PathCardDTO> LoadPathCards(List<PathCardDTO> pathCardDtos)
    {
        if (pathCardDtos == null) return new Dictionary<string, PathCardDTO>();
        
        Console.WriteLine($"[PackageLoader] Loading {pathCardDtos.Count} path cards...");
        
        var pathCardLookup = new Dictionary<string, PathCardDTO>();
        foreach (PathCardDTO dto in pathCardDtos)
        {
            pathCardLookup[dto.Id] = dto;
            Console.WriteLine($"[PackageLoader] Loaded path card '{dto.Id}': {dto.Name}");
        }
        
        Console.WriteLine($"[PackageLoader] Completed loading path cards. Total: {pathCardLookup.Count}");
        return pathCardLookup;
    }

    private Dictionary<string, PathCardDTO> LoadEventCards(List<PathCardDTO> eventCardDtos)
    {
        if (eventCardDtos == null) return new Dictionary<string, PathCardDTO>();
        
        Console.WriteLine($"[PackageLoader] Loading {eventCardDtos.Count} event cards...");
        
        var eventCardLookup = new Dictionary<string, PathCardDTO>();
        foreach (PathCardDTO dto in eventCardDtos)
        {
            eventCardLookup[dto.Id] = dto;
            Console.WriteLine($"[PackageLoader] Loaded event card '{dto.Id}': {dto.Name}");
        }
        
        Console.WriteLine($"[PackageLoader] Completed loading event cards. Total: {eventCardLookup.Count}");
        return eventCardLookup;
    }

    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, Dictionary<string, PathCardDTO> eventCardLookup)
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
                    if (eventCardLookup.TryGetValue(cardId, out PathCardDTO eventCard))
                    {
                        dto.EventCards.Add(eventCard);
                    }
                }
            }
            
            _gameWorld.AllTravelEvents[dto.Id] = dto;
            Console.WriteLine($"[PackageLoader] Loaded travel event '{dto.Id}': {dto.Name} with {dto.EventCards.Count} event cards");
        }
        
        Console.WriteLine($"[PackageLoader] Completed loading travel events. Total: {_gameWorld.AllTravelEvents.Count}");
    }
    
    
    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, Dictionary<string, PathCardDTO> pathCardLookup, Dictionary<string, PathCardDTO> eventCardLookup)
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
                    if (pathCardLookup.TryGetValue(cardId, out PathCardDTO pathCard))
                    {
                        dto.PathCards.Add(pathCard);
                    }
                }
            }
            
            // Determine if this is a path collection or event collection based on contents
            bool isEventCollection = (dto.Events != null && dto.Events.Count > 0);
            bool isPathCollection = (dto.PathCards != null && dto.PathCards.Count > 0);
            
            if (isEventCollection)
            {
                // This is an event collection - contains child events for random selection
                _gameWorld.AllEventCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded event collection '{dto.Id}': {dto.Name} with {dto.Events.Count} events");
            }
            else if (isPathCollection)
            {
                // This is a path collection - contains actual path cards for FixedPath segments
                _gameWorld.AllPathCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded path collection '{dto.Id}': {dto.Name} with {dto.PathCards.Count} path cards");
            }
            else
            {
                // Fallback: treat as path collection if no clear indicators
                _gameWorld.AllPathCollections[dto.Id] = dto;
                Console.WriteLine($"[PackageLoader] Loaded collection '{dto.Id}' as path collection (default): {dto.Name}");
            }
        }

        Console.WriteLine($"[PackageLoader] Completed loading collections. Path collections: {_gameWorld.AllPathCollections.Count}, Event collections: {_gameWorld.AllEventCollections.Count}");
    }

    /// <summary>
    /// Initialize conversation decks for all NPCs with the universal starter deck
    /// </summary>
    private void InitializeNPCConversationDecks(DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC conversation decks with composition rules...");

        if (deckCompositions == null)
        {
            Console.WriteLine("[PackageLoader] Error: No deck compositions defined in package");
            return;
        }

        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                npc.ConversationDeck = new CardDeck();

                // Check for NPC-specific deck first
                DeckDefinitionDTO deckDef = null;
                if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                {
                    deckDef = deckCompositions.NpcDecks[npc.ID];
                    Console.WriteLine($"[PackageLoader] Using custom deck for {npc.Name}");
                }
                else if (deckCompositions.DefaultDeck != null)
                {
                    deckDef = deckCompositions.DefaultDeck;
                    Console.WriteLine($"[PackageLoader] Using default deck for {npc.Name}");
                }

                if (deckDef?.ConversationDeck != null)
                {
                    // Add cards according to composition
                    foreach (KeyValuePair<string, int> kvp in deckDef.ConversationDeck)
                    {
                        string cardId = kvp.Key;
                        int count = kvp.Value;

                        if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                        {
                            ConversationCard cardTemplate = _gameWorld.AllCardDefinitions[cardId] as ConversationCard;
                            if (cardTemplate != null && !(cardTemplate.CardType == CardType.Letter || cardTemplate.CardType == CardType.Promise || cardTemplate.CardType == CardType.BurdenGoal))
                            {
                                // Add multiple copies as specified
                                for (int i = 0; i < count; i++)
                                {
                                    npc.ConversationDeck.AddCard(cardTemplate);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[PackageLoader] Warning: Card '{cardId}' not found for deck composition");
                        }
                    }

                    Console.WriteLine($"[PackageLoader] Initialized {npc.Name}'s conversation deck with {npc.ConversationDeck.Count} cards");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PackageLoader] Failed to initialize conversation deck for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC conversation deck initialization completed");
    }


    /// <summary>
    /// Initialize one-time requests for NPCs from NpcGoalCards and deck compositions
    /// </summary>
    private void InitializeNPCRequests(List<NPCRequestDTO> npcRequestDtos, List<NPCGoalCardDTO> npcGoalCardDtos, DeckCompositionDTO deckCompositions)
    {
        Console.WriteLine("[PackageLoader] Initializing NPC one-time requests...");

        // First load NPCRequest bundles from npcRequests section
        if (npcRequestDtos != null && npcRequestDtos.Count > 0)
        {
            foreach (NPCRequestDTO requestDto in npcRequestDtos)
            {
                try
                {
                    NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == requestDto.NpcId);
                    if (npc == null)
                    {
                        Console.WriteLine($"[PackageLoader] Warning: NPC '{requestDto.NpcId}' not found for request '{requestDto.Id}'");
                        continue;
                    }

                    NPCRequest request = new NPCRequest
                    {
                        Id = requestDto.Id,
                        Name = requestDto.Name,
                        Description = requestDto.Description,
                        Status = RequestStatus.Available
                    };

                    // Load request cards
                    if (requestDto.RequestCards != null)
                    {
                        foreach (string cardId in requestDto.RequestCards)
                        {
                            if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard card))
                            {
                                request.RequestCards.Add(card);
                                Console.WriteLine($"[PackageLoader] Added request card '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                Console.WriteLine($"[PackageLoader] Warning: Request card '{cardId}' not found");
                            }
                        }
                    }

                    // Load promise cards
                    if (requestDto.PromiseCards != null)
                    {
                        foreach (string cardId in requestDto.PromiseCards)
                        {
                            if (_gameWorld.AllCardDefinitions.TryGetValue(cardId, out ConversationCard card))
                            {
                                request.PromiseCards.Add(card);
                                Console.WriteLine($"[PackageLoader] Added promise card '{cardId}' to request '{requestDto.Id}'");
                            }
                            else
                            {
                                Console.WriteLine($"[PackageLoader] Warning: Promise card '{cardId}' not found");
                            }
                        }
                    }

                    if (request.RequestCards.Count > 0 || request.PromiseCards.Count > 0)
                    {
                        npc.Requests.Add(request);
                        Console.WriteLine($"[PackageLoader] Added request '{requestDto.Id}' to NPC '{npc.Name}' with {request.RequestCards.Count} request cards and {request.PromiseCards.Count} promise cards");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PackageLoader] Failed to load request '{requestDto.Id}': {ex.Message}");
                }
            }
            
            // Skip legacy loading if we have proper request bundles
            Console.WriteLine($"[PackageLoader] Loaded {npcRequestDtos.Count} NPC request bundles");
            return;
        }

        // Legacy fallback: Group goal cards by NPC ID for efficient loading
        Dictionary<string, List<ConversationCard>> npcGoalCards = new Dictionary<string, List<ConversationCard>>();

        // Load goal cards from the NpcGoalCards section if it exists
        if (npcGoalCardDtos != null)
        {
            foreach (NPCGoalCardDTO goalCardDto in npcGoalCardDtos)
            {
                try
                {
                    // Convert DTO to ConversationCard using ConversationCardParser
                    ConversationCard conversationCard = ConversationCardParser.ConvertGoalCardDTO(goalCardDto);

                    // Add to NPC's collection
                    if (!npcGoalCards.ContainsKey(goalCardDto.NpcId))
                    {
                        npcGoalCards[goalCardDto.NpcId] = new List<ConversationCard>();
                    }
                    npcGoalCards[goalCardDto.NpcId].Add(conversationCard);

                    Console.WriteLine($"[PackageLoader] Loaded goal card '{goalCardDto.Id}' for NPC '{goalCardDto.NpcId}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PackageLoader] Failed to load goal card '{goalCardDto.Id}': {ex.Message}");
                }
            }
        }

        // Initialize request decks for all NPCs
        foreach (NPC npc in _gameWorld.NPCs)
        {
            try
            {
                List<ConversationCard> requestCards = new List<ConversationCard>();

                // Load cards from deck compositions (like conversation decks)
                if (deckCompositions != null)
                {
                    // Check for NPC-specific deck first
                    DeckDefinitionDTO deckDef = null;
                    if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                    {
                        deckDef = deckCompositions.NpcDecks[npc.ID];
                        Console.WriteLine($"[PackageLoader] Using custom request deck composition for {npc.Name}");
                    }
                    else if (deckCompositions.DefaultDeck != null)
                    {
                        deckDef = deckCompositions.DefaultDeck;
                        Console.WriteLine($"[PackageLoader] Using default request deck composition for {npc.Name}");
                    }

                    if (deckDef?.RequestDeck != null && deckDef.RequestDeck.Count > 0)
                    {
                        // Add cards according to composition
                        foreach (KeyValuePair<string, int> kvp in deckDef.RequestDeck)
                        {
                            string cardId = kvp.Key;
                            int count = kvp.Value;

                            if (_gameWorld.AllCardDefinitions.ContainsKey(cardId))
                            {
                                ConversationCard cardTemplate = _gameWorld.AllCardDefinitions[cardId] as ConversationCard;
                                if (cardTemplate != null)
                                {
                                    // Add multiple copies as specified
                                    for (int i = 0; i < count; i++)
                                    {
                                        requestCards.Add(cardTemplate);
                                    }
                                    Console.WriteLine($"[PackageLoader] Added {count}x '{cardId}' to {npc.Name}'s request deck");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"[PackageLoader] Warning: Request card '{cardId}' not found in AllCardDefinitions");
                            }
                        }
                    }
                }

                // Add NPC-specific goal cards if any exist (from npcGoalCards section)
                if (npcGoalCards.ContainsKey(npc.ID))
                {
                    requestCards.AddRange(npcGoalCards[npc.ID]);
                    Console.WriteLine($"[PackageLoader] Added {npcGoalCards[npc.ID].Count} goal cards to {npc.Name}'s one-time requests");
                }

                // Populate Requests ONLY
                if (requestCards.Count > 0)
                {
                    // Group cards by RequestId or create a default request
                    var defaultRequest = new NPCRequest
                    {
                        Id = $"{npc.ID}_default_request",
                        Name = "Available Requests",
                        Description = "Requests available from this NPC",
                        Status = RequestStatus.Available
                    };
                    
                    // Separate promise cards from regular request cards
                    foreach (var card in requestCards)
                    {
                        if (card.CardType == CardType.Promise)
                        {
                            defaultRequest.PromiseCards.Add(card);
                        }
                        else if (card.CardType == CardType.Letter || card.CardType == CardType.BurdenGoal)
                        {
                            defaultRequest.RequestCards.Add(card);
                        }
                    }
                    
                    if (defaultRequest.RequestCards.Count > 0 || defaultRequest.PromiseCards.Count > 0)
                    {
                        npc.Requests.Add(defaultRequest);
                        Console.WriteLine($"[PackageLoader] Created default request for {npc.Name} with {defaultRequest.RequestCards.Count} request cards and {defaultRequest.PromiseCards.Count} promise cards");
                    }
                }
                
                Console.WriteLine($"[PackageLoader] Initialized one-time requests for {npc.Name} with {requestCards.Count} cards");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PackageLoader] Failed to initialize one-time requests for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC one-time requests initialization completed");
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
                DeckDefinitionDTO deckDef = null;
                if (deckCompositions != null)
                {
                    // Check for NPC-specific deck first
                    if (deckCompositions.NpcDecks != null && deckCompositions.NpcDecks.ContainsKey(npc.ID))
                    {
                        deckDef = deckCompositions.NpcDecks[npc.ID];
                        Console.WriteLine($"[PackageLoader] Using custom exchange deck for {npc.Name}");
                    }
                    else if (deckCompositions.DefaultDeck != null)
                    {
                        deckDef = deckCompositions.DefaultDeck;
                        Console.WriteLine($"[PackageLoader] Using default exchange deck for {npc.Name}");
                    }
                }

                // Build exchange deck from composition
                if (deckDef != null && deckDef.ExchangeDeck != null)
                {
                    foreach (var kvp in deckDef.ExchangeDeck)
                    {
                        string cardId = kvp.Key;
                        int count = kvp.Value;
                        
                        // Find the exchange card from the parsed exchange cards
                        if (_parsedExchangeCards != null && _parsedExchangeCards.ContainsKey(cardId))
                        {
                            ExchangeCard exchangeCard = _parsedExchangeCards[cardId];
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
                Console.WriteLine($"[PackageLoader] Failed to initialize exchange deck for NPC {npc.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("[PackageLoader] NPC exchange deck initialization completed");
    }

    private void LoadItems(List<ItemDTO> itemDtos)
    {
        if (itemDtos == null) return;

        foreach (ItemDTO dto in itemDtos)
        {
            Item item = ItemParser.ConvertDTOToItem(dto);
            _gameWorld.WorldState.Items.Add(item);
        }
    }

    private void LoadLetterTemplates(List<LetterTemplateDTO> letterDtos)
    {
        if (letterDtos == null) return;

        foreach (LetterTemplateDTO dto in letterDtos)
        {
            LetterTemplate letter = LetterTemplateParser.ConvertDTOToLetterTemplate(dto);
            _gameWorld.WorldState.LetterTemplates.Add(letter);
        }
    }

    private void LoadStandingObligations(List<StandingObligationDTO> obligationDtos)
    {
        if (obligationDtos == null) return;

        foreach (StandingObligationDTO dto in obligationDtos)
        {
            StandingObligation obligation = StandingObligationParser.ConvertDTOToStandingObligation(dto);
            _gameWorld.WorldState.StandingObligationTemplates.Add(obligation);
        }
    }

    private void LoadLocationActions(List<LocationActionDTO> locationActionDtos)
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
        ConversationCard card = new ConversationCard
        {
            Id = dto.Id,
            Description = dto.DisplayText ?? "",
            Focus = dto.Focus,
            TokenType = ConnectionType.Trust,
            Difficulty = Difficulty.Medium
        };
        card.CardType = CardType.Observation;
        card.Properties.Add(CardProperty.Persistent);
        return card;
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
            Icon = dto.Icon ?? "💼",
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

    private void LoadInvestigationRewards(List<ObservationRewardDTO> investigationRewardDtos)
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
                _gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
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

    private void LoadExchanges(List<ExchangeDTO> exchangeDtos)
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
        _parsedExchangeCards = new Dictionary<string, ExchangeCard>();
        foreach (ExchangeDTO dto in exchangeDtos)
        {
            ExchangeCard exchangeCard = ExchangeParser.ParseExchange(dto);
            _parsedExchangeCards[exchangeCard.Id] = exchangeCard;
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
        foreach (var collection in _gameWorld.AllPathCollections.Values)
        {
            foreach (var pathCard in collection.PathCards)
            {
                _gameWorld.PathCardDiscoveries[pathCard.Id] = pathCard.StartsRevealed;
                Console.WriteLine($"[PackageLoader] Path card '{pathCard.Id}' discovery state: {(pathCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }
        
        // Also initialize discovery states for event cards in event collections
        foreach (var collection in _gameWorld.AllEventCollections.Values)
        {
            foreach (var eventCard in collection.EventCards)
            {
                _gameWorld.PathCardDiscoveries[eventCard.Id] = eventCard.StartsRevealed;
                Console.WriteLine($"[PackageLoader] Event card '{eventCard.Id}' discovery state: {(eventCard.StartsRevealed ? "face-up" : "face-down")}");
            }
        }
        
        // Cards are now embedded directly in collections
        // No separate card dictionaries needed

        // Initialize EventDeckPositions for routes with event pools
        foreach (KeyValuePair<string, PathCardCollectionDTO> kvp in _gameWorld.AllEventCollections)
        {
            string routeId = kvp.Key;
            string deckKey = $"route_{routeId}_events";
            
            // Start at position 0 for deterministic event drawing
            _gameWorld.EventDeckPositions[deckKey] = 0;
            
            int eventCount = kvp.Value.Events?.Count ?? 0;
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
        foreach (var kvp in forwardRoute.WeatherModifications)
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
        var reversedSegments = forwardRoute.Segments.OrderByDescending(s => s.SegmentNumber).ToList();
        int segmentNumber = 1;
        foreach (var originalSegment in reversedSegments)
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
        if (_gameWorld.AllEventCollections.ContainsKey(forwardRoute.Id))
        {
            _gameWorld.AllEventCollections[reverseRoute.Id] = _gameWorld.AllEventCollections[forwardRoute.Id];
        }
        
        return reverseRoute;
    }
    
    private string GetLocationNameFromId(string locationId)
    {
        // Helper to get friendly location name from ID for route naming
        var location = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);
        return location?.Name ?? locationId.Replace("_", " ").Replace("-", " ");
    }

    private void ValidateCrossroadsConfiguration()
    {
        Console.WriteLine("[PackageLoader] Starting crossroads configuration validation...");
        
        // Group spots by location
        Dictionary<string, List<LocationSpot>> spotsByLocation = new Dictionary<string, List<LocationSpot>>();
        foreach (LocationSpot spot in _gameWorld.WorldState.locationSpots)
        {
            if (!spotsByLocation.ContainsKey(spot.LocationId))
            {
                spotsByLocation[spot.LocationId] = new List<LocationSpot>();
            }
            spotsByLocation[spot.LocationId].Add(spot);
        }

        // Validate each location has exactly one crossroads spot
        foreach (Location location in _gameWorld.WorldState.locations)
        {
            if (!spotsByLocation.ContainsKey(location.Id))
            {
                throw new InvalidOperationException($"Location '{location.Id}' ({location.Name}) has no spots defined");
            }

            List<LocationSpot> locationSpots = spotsByLocation[location.Id];
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

            Console.WriteLine($"[PackageLoader] ✓ Location '{location.Id}' has valid crossroads spot: '{crossroadsSpots[0].SpotID}'");
        }

        // Validate all route spots have crossroads property
        HashSet<string> routeSpotIds = new HashSet<string>();
        foreach (RouteOption route in _gameWorld.WorldState.Routes)
        {
            routeSpotIds.Add(route.OriginLocationSpot);
            routeSpotIds.Add(route.DestinationLocationSpot);
        }

        foreach (string spotId in routeSpotIds)
        {
            LocationSpot spot = _gameWorld.WorldState.locationSpots.FirstOrDefault(s => s.SpotID == spotId);
            if (spot == null)
            {
                throw new InvalidOperationException($"Route references spot '{spotId}' which does not exist");
            }

            if (!spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true)
            {
                throw new InvalidOperationException($"Route spot '{spotId}' ({spot.Name}) does not have Crossroads property. All route origin and destination spots must be crossroads.");
            }

            Console.WriteLine($"[PackageLoader] ✓ Route spot '{spotId}' has valid Crossroads property");
        }

        Console.WriteLine($"[PackageLoader] Crossroads validation completed successfully. Validated {_gameWorld.WorldState.locations.Count} locations and {routeSpotIds.Count} route spots.");
    }
}