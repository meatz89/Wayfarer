using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
public class GameWorldInitializer : IGameWorldFactory
{
    private readonly IContentDirectory _contentDirectory;
    private readonly LocationFactory _locationFactory;
    private readonly LocationSpotFactory _locationSpotFactory;
    private readonly NPCFactory _npcFactory;
    private readonly ItemFactory _itemFactory;
    private readonly RouteFactory _routeFactory;
    private readonly RouteDiscoveryFactory _routeDiscoveryFactory;
    private readonly NetworkUnlockFactory _networkUnlockFactory;
    private readonly LetterTemplateFactory _letterTemplateFactory;
    private readonly StandingObligationFactory _standingObligationFactory;

    public GameWorldInitializer(
        IContentDirectory contentDirectory,
        LocationFactory locationFactory,
        LocationSpotFactory locationSpotFactory,
        NPCFactory npcFactory,
        ItemFactory itemFactory,
        RouteFactory routeFactory,
        RouteDiscoveryFactory routeDiscoveryFactory,
        NetworkUnlockFactory networkUnlockFactory,
        LetterTemplateFactory letterTemplateFactory,
        StandingObligationFactory standingObligationFactory)
    {
        _contentDirectory = contentDirectory ?? throw new ArgumentNullException(nameof(contentDirectory));
        _locationFactory = locationFactory ?? throw new ArgumentNullException(nameof(locationFactory));
        _locationSpotFactory = locationSpotFactory ?? throw new ArgumentNullException(nameof(locationSpotFactory));
        _npcFactory = npcFactory ?? throw new ArgumentNullException(nameof(npcFactory));
        _itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
        _routeFactory = routeFactory ?? throw new ArgumentNullException(nameof(routeFactory));
        _routeDiscoveryFactory = routeDiscoveryFactory ?? throw new ArgumentNullException(nameof(routeDiscoveryFactory));
        _networkUnlockFactory = networkUnlockFactory ?? throw new ArgumentNullException(nameof(networkUnlockFactory));
        _letterTemplateFactory = letterTemplateFactory ?? throw new ArgumentNullException(nameof(letterTemplateFactory));
        _standingObligationFactory = standingObligationFactory ?? throw new ArgumentNullException(nameof(standingObligationFactory));
    }

    public GameWorld LoadGame()
    {
        return LoadGameFromTemplates();
    }
    
    /// <summary>
    /// IGameWorldFactory implementation
    /// </summary>
    public GameWorld CreateGameWorld()
    {
        return LoadGame();
    }

    private GameWorld LoadGameFromTemplates()
    {
        string templatePath = Path.Combine(_contentDirectory.Path, "Templates");

        // PHASE 1: Load entities without references (Locations, Items)
        Console.WriteLine("\n=== PHASE 1: Loading base entities ===");
        List<Location> locations = LoadLocations(templatePath);
        List<Item> items = LoadItems(templatePath);
        
        // PHASE 2: Create initial GameWorld with base entities
        // Actions removed - using letter queue system
        
        // Skill cards removed - using letter queue system
        
        // Create GameWorld with base entities
        GameWorld gameWorld = GameWorldSerializer.DeserializeGameWorld(
            File.ReadAllText(Path.Combine(templatePath, "gameWorld.json")),
            locations, new List<LocationSpot>());
        
        // Add items to GameWorld
        if (gameWorld.WorldState.Items == null)
        {
            gameWorld.WorldState.Items = new List<Item>();
        }
        gameWorld.WorldState.Items.AddRange(items);
        
        // PHASE 3: Load entities with simple references (LocationSpots, NPCs)
        Console.WriteLine("\n=== PHASE 3: Loading entities with references ===");
        List<LocationSpot> spots = LoadLocationSpots(templatePath, gameWorld);
        gameWorld.WorldState.locationSpots.AddRange(spots);
        
        List<NPC> npcs = LoadNPCs(templatePath, locations);
        foreach (var npc in npcs)
        {
            gameWorld.WorldState.AddCharacter(npc);
        }
        
        // Action system removed - location actions handled by LocationActionManager
        
        // PHASE 4: Connect entities
        Console.WriteLine("\n=== PHASE 4: Connecting entities ===");
        ConnectLocationsToSpots(locations, spots);
        ConnectNPCsToLocationSpots(npcs, spots);
        
        // PHASE 5: Load routes (reference locations)
        Console.WriteLine("\n=== PHASE 5: Loading routes ===");
        List<RouteOption> routes = LoadRoutes(templatePath, locations);
        ConnectRoutesToLocations(locations, routes);
        
        if (gameWorld.DiscoveredRoutes == null)
        {
            gameWorld.DiscoveredRoutes = new List<RouteOption>();
        }
        gameWorld.DiscoveredRoutes.AddRange(routes);

        // Continue loading other content

        // Load letter templates with validated NPC references
        List<LetterTemplate> letterTemplates = LoadLetterTemplates(templatePath, npcs);

        // Load standing obligations with validated NPC references
        List<StandingObligation> standingObligations = LoadStandingObligations(templatePath, npcs);

        // NPCs already added in Phase 3
        if (!npcs.Any())
        {
            Console.WriteLine("INFO: No NPCs loaded from JSON templates. Create npcs.json file to add NPC content.");
        }

        // Connect NPCs to their location spots
        ConnectNPCsToLocationSpots(npcs, gameWorld.WorldState.locationSpots);


        // Add letter templates to the game world
        if (letterTemplates.Any())
        {
            gameWorld.WorldState.LetterTemplates.AddRange(letterTemplates);
            Console.WriteLine($"Added {letterTemplates.Count} letter templates to game world.");
        }
        else
        {
            Console.WriteLine("INFO: No letter templates loaded. Create letter_templates.json file to add letter template content.");
        }

        // Add standing obligations to the game world
        if (standingObligations.Any())
        {
            gameWorld.WorldState.StandingObligationTemplates.AddRange(standingObligations);
            Console.WriteLine($"Added {standingObligations.Count} standing obligation templates to game world.");
        }
        else
        {
            Console.WriteLine("INFO: No standing obligations loaded. Create standing_obligations.json file to add obligation content.");
        }
        
        // PHASE 2: Load progression content that references entities
        LoadProgressionContent(gameWorld, templatePath);

        // Initialize player inventory if not already initialized
        if (gameWorld.GetPlayer().Inventory == null)
        {
            gameWorld.GetPlayer().Inventory = new Inventory(10);
        }

        // Add routes to the game world
        if (gameWorld.DiscoveredRoutes == null)
        {
            gameWorld.DiscoveredRoutes = new List<RouteOption>();
        }
        gameWorld.DiscoveredRoutes.AddRange(routes);

        // Contract system removed - using letter system instead

        // CRITICAL: Ensure Player.CurrentLocation and CurrentLocationSpot are NEVER null
        // Systems depend on these values being valid
        InitializePlayerLocation(gameWorld);

        return gameWorld;
    }
    
    /// <summary>
    /// Phase 2 loading: Load progression content that references entities.
    /// This must happen after all entities (NPCs, routes, items) are loaded.
    /// </summary>
    private void LoadProgressionContent(GameWorld gameWorld, string templatePath)
    {
        // Load route discoveries
        LoadRouteDiscoveries(gameWorld, templatePath);
        
        // Load network unlocks
        LoadNetworkUnlocks(gameWorld, templatePath);
        
        // Load token favors
        LoadTokenFavors(gameWorld, templatePath);
    }
    
    private void LoadRouteDiscoveries(GameWorld gameWorld, string templatePath)
    {
        string progressionPath = Path.Combine(templatePath, "Progression");
        string routeDiscoveryPath = Path.Combine(progressionPath, "route_discovery.json");
        
        if (!File.Exists(routeDiscoveryPath))
        {
            Console.WriteLine("INFO: No route discoveries loaded. Create route_discovery.json to add route discovery content.");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(routeDiscoveryPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var discoveries = JsonSerializer.Deserialize<List<RouteDiscoveryDTO>>(json, options);
            
            if (discoveries == null || !discoveries.Any())
            {
                Console.WriteLine("WARNING: route_discovery.json exists but contains no route discoveries.");
                return;
            }
            
            int successCount = 0;
            foreach (var dto in discoveries)
            {
                try
                {
                    var discovery = _routeDiscoveryFactory.CreateRouteDiscoveryFromIds(
                        dto.RouteId,
                        dto.KnownByNPCs,
                        gameWorld.DiscoveredRoutes,
                        gameWorld.WorldState.NPCs,
                        dto.RequiredTokensWithNPC);
                    
                    // Add discovery contexts
                    foreach (var (npcId, contextDto) in dto.DiscoveryContexts)
                    {
                        _routeDiscoveryFactory.AddDiscoveryContextFromIds(
                            discovery,
                            npcId,
                            contextDto.RequiredEquipment,
                            gameWorld.WorldState.NPCs,
                            gameWorld.WorldState.Items,
                            contextDto.Narrative);
                    }
                    
                    gameWorld.WorldState.RouteDiscoveries.Add(discovery);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to load route discovery for route '{dto.RouteId}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {successCount} route discoveries from JSON templates.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to load route discoveries from JSON: {ex.Message}");
        }
    }
    
    private void LoadNetworkUnlocks(GameWorld gameWorld, string templatePath)
    {
        string networkUnlockPath = Path.Combine(templatePath, "progression_unlocks.json");
        
        if (!File.Exists(networkUnlockPath))
        {
            Console.WriteLine("INFO: No network unlocks loaded. Create progression_unlocks.json to add network unlock content.");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(networkUnlockPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var unlocks = JsonSerializer.Deserialize<List<NetworkUnlockDTO>>(json, options);
            
            if (unlocks == null || !unlocks.Any())
            {
                Console.WriteLine("WARNING: progression_unlocks.json exists but contains no network unlocks.");
                return;
            }
            
            int successCount = 0;
            foreach (var dto in unlocks)
            {
                try
                {
                    var targetDefinitions = dto.Unlocks
                        .Select(t => (t.NpcId, t.IntroductionText))
                        .ToList();
                    
                    var networkUnlock = _networkUnlockFactory.CreateNetworkUnlockFromIds(
                        dto.Id,
                        dto.UnlockerNpcId,
                        dto.TokensRequired,
                        dto.UnlockDescription,
                        targetDefinitions,
                        gameWorld.WorldState.NPCs);
                    
                    gameWorld.WorldState.NetworkUnlocks.Add(networkUnlock);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to load network unlock '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {successCount} network unlocks from JSON templates.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to load network unlocks from JSON: {ex.Message}");
        }
    }
    
    private void LoadTokenFavors(GameWorld gameWorld, string templatePath)
    {
        string tokenFavorsPath = Path.Combine(templatePath, "token_favors.json");
        
        if (!File.Exists(tokenFavorsPath))
        {
            Console.WriteLine("INFO: No token favors loaded. Create token_favors.json to add token favor content.");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(tokenFavorsPath);
            var favors = TokenFavorParser.ParseTokenFavorArray(json);
            
            if (favors == null || !favors.Any())
            {
                Console.WriteLine("INFO: No token favors found in token_favors.json");
                return;
            }
            
            // Validate NPC references
            var availableNPCs = gameWorld.WorldState.NPCs;
            var validFavors = new List<TokenFavor>();
            
            foreach (var favor in favors)
            {
                if (!availableNPCs.Any(n => n.ID == favor.NPCId))
                {
                    Console.WriteLine($"WARNING: Token favor '{favor.Id}' references unknown NPC '{favor.NPCId}' - skipping");
                    continue;
                }
                
                // Additional validation based on favor type
                bool isValid = ValidateTokenFavor(favor, gameWorld);
                if (isValid)
                {
                    validFavors.Add(favor);
                }
            }
            
            gameWorld.WorldState.TokenFavors = validFavors;
            Console.WriteLine($"Loaded {validFavors.Count} token favors from JSON templates.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to load token favors from JSON: {ex.Message}");
        }
    }
    
    private bool ValidateTokenFavor(TokenFavor favor, GameWorld gameWorld)
    {
        switch (favor.FavorType)
        {
            case TokenFavorType.RouteDiscovery:
                if (!gameWorld.WorldState.Routes.Any(r => r.Id == favor.GrantsId))
                {
                    Console.WriteLine($"WARNING: Token favor '{favor.Id}' references unknown route '{favor.GrantsId}'");
                    return false;
                }
                break;
                
            case TokenFavorType.ItemPurchase:
                if (!gameWorld.WorldState.Items.Any(i => i.Id == favor.GrantsId))
                {
                    Console.WriteLine($"WARNING: Token favor '{favor.Id}' references unknown item '{favor.GrantsId}'");
                    return false;
                }
                break;
                
            case TokenFavorType.LocationAccess:
                if (!gameWorld.WorldState.locations.Any(l => l.Id == favor.GrantsId))
                {
                    Console.WriteLine($"WARNING: Token favor '{favor.Id}' references unknown location '{favor.GrantsId}'");
                    return false;
                }
                break;
                
            case TokenFavorType.NPCIntroduction:
                if (!gameWorld.WorldState.NPCs.Any(n => n.ID == favor.GrantsId))
                {
                    Console.WriteLine($"WARNING: Token favor '{favor.Id}' references unknown NPC '{favor.GrantsId}'");
                    return false;
                }
                break;
        }
        
        return true;
    }

    /// <summary>
    /// CRITICAL: Ensure Player.CurrentLocation and CurrentLocationSpot are NEVER null
    /// Systems depend on these values being valid at all times
    /// </summary>
    private void InitializePlayerLocation(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        WorldState worldState = gameWorld.WorldState;

        // If WorldState has current location but Player doesn't, sync them
        if (worldState.CurrentLocation != null && player.CurrentLocation == null)
        {
            player.CurrentLocation = worldState.CurrentLocation;
            Console.WriteLine($"Set player CurrentLocation to: {worldState.CurrentLocation.Id}");
        }

        if (worldState.CurrentLocationSpot != null && player.CurrentLocationSpot == null)
        {
            player.CurrentLocationSpot = worldState.CurrentLocationSpot;
            Console.WriteLine($"Set player CurrentLocationSpot to: {worldState.CurrentLocationSpot.SpotID}");
        }

        // If neither has location, set to first available location
        if (player.CurrentLocation == null && worldState.locations.Any())
        {
            Location firstLocation = worldState.locations.First();
            LocationSpot firstSpot = worldState.locationSpots.FirstOrDefault(s => s.LocationId == firstLocation.Id);

            if (firstSpot != null)
            {
                player.CurrentLocation = firstLocation;
                player.CurrentLocationSpot = firstSpot;
                worldState.SetCurrentLocation(firstLocation, firstSpot);
                Console.WriteLine($"FALLBACK: Set player location to: {firstLocation.Id}, spot: {firstSpot.SpotID}");
            }
        }

        // Final validation - these should NEVER be null
        if (player.CurrentLocation == null || player.CurrentLocationSpot == null)
        {
            throw new InvalidOperationException("CRITICAL: Player location initialization failed. CurrentLocation and CurrentLocationSpot must never be null.");
        }
    }

    // Helper method to connect routes to locations
    private void ConnectRoutesToLocations(List<Location> locations, List<RouteOption> routes)
    {
        foreach (Location location in locations)
        {
            // For each connected location, create a LocationConnection
            foreach (string connectedLocationId in location.ConnectedLocationIds)
            {
                // Find the destination location
                Location destinationLocation = locations.FirstOrDefault(l => l.Id == connectedLocationId);
                if (destinationLocation == null) continue;

                // Create a new location connection
                LocationConnection connection = new LocationConnection
                {
                    DestinationLocationId = connectedLocationId
                };

                // Find all routes that connect these locations
                foreach (RouteOption route in routes)
                {
                    if (route.Origin == location.Id && route.Destination == connectedLocationId)
                    {
                        // Add route to the connection
                        connection.RouteOptions.Add(route);
                    }
                }

                // If no specific routes were found, create a default walking route
                if (connection.RouteOptions.Count == 0)
                {
                    RouteOption defaultRoute = new RouteOption
                    {
                        Id = $"{location.Id}_to_{connectedLocationId}_walk",
                        Name = $"Walk to {destinationLocation.Name}",
                        Origin = location.Id,
                        Destination = connectedLocationId,
                        Method = TravelMethods.Walking,
                        BaseCoinCost = 0,
                        BaseStaminaCost = 2,
                        TravelTimeHours = 3,
                        DepartureTime = null,
                        IsDiscovered = true,
                        MaxItemCapacity = 3,
                        Description = $"A walk from {location.Name} to {destinationLocation.Name}."
                    };
                    connection.RouteOptions.Add(defaultRoute);

                    // Also add this default route to the routes list
                    routes.Add(defaultRoute);
                }

                // Add the connection to the location
                location.Connections.Add(connection);
            }
        }
    }

    // Helper method to connect NPCs to their location spots
    private void ConnectNPCsToLocationSpots(List<NPC> npcs, List<LocationSpot> spots)
    {
        // Create mapping of NPCs to CHARACTER type location spots
        List<LocationSpot> characterSpots = spots.Where(s => s.Type == LocationSpotTypes.CHARACTER).ToList();

        foreach (LocationSpot characterSpot in characterSpots)
        {
            // Try to find a matching NPC based on location and role/profession
            NPC matchingNPC = null;

            // Strategy 1: Try to match by location and profession
            if (characterSpot.SpotID == "innkeeper" && characterSpot.LocationId == "dusty_flagon")
            {
                matchingNPC = npcs.FirstOrDefault(n => n.Location == "dusty_flagon" && n.Profession == Professions.Merchant);
            }
            else if (characterSpot.SpotID == "market_stall" && characterSpot.LocationId == "town_square")
            {
                matchingNPC = npcs.FirstOrDefault(n => n.Location == "town_square" && n.Profession == Professions.Merchant);
            }

            // Strategy 2: General location-based matching for other spots
            if (matchingNPC == null)
            {
                matchingNPC = npcs.FirstOrDefault(n => n.Location == characterSpot.LocationId);
            }

            // Connect the NPC to the spot
            if (matchingNPC != null)
            {
                characterSpot.PrimaryNPC = matchingNPC;
                Console.WriteLine($"Connected NPC '{matchingNPC.Name}' to location spot '{characterSpot.SpotID}' at {characterSpot.LocationId}");
            }
            else
            {
                Console.WriteLine($"WARNING: No matching NPC found for CHARACTER spot '{characterSpot.SpotID}' at {characterSpot.LocationId}");
            }
        }
    }

    private GameWorld CreateInitialGameWorld()
    {
        GameWorld gameWorld = new GameWorld();

        // Load fresh content from the content directory
        List<Location> locations = new List<Location>();
        List<LocationSpot> spots = new List<LocationSpot>();
        locations = ConnectLocationsToSpots(locations, spots);

        // Action and skill systems removed - using letter system instead

        // Add content to game state
        gameWorld.WorldState.locations.Clear();
        gameWorld.WorldState.locations.AddRange(locations);

        gameWorld.WorldState.locationSpots.Clear();
        gameWorld.WorldState.locationSpots.AddRange(spots);

        // Actions removed - using letter queue system
        // Contract system removed - using letter system instead

        // Card system removed - using letter queue system

        return gameWorld;
    }

    private List<Location> ConnectLocationsToSpots(List<Location> locations, List<LocationSpot> spots)
    {
        foreach (Location location in locations)
        {
            foreach (string locSpotId in location.LocationSpotIds)
            {
                LocationSpot? spot = spots.FirstOrDefault(s => s.SpotID == locSpotId);
                if (spot != null)
                {
                    location.AvailableSpots.Add(spot);
                    spot.LocationId = location.Id;
                }
            }
        }

        return locations;
    }

    private List<NPC> ParseNPCArray(string npcsJson)
    {
        List<NPC> npcs = new List<NPC>();

        using (JsonDocument doc = JsonDocument.Parse(npcsJson))
        {
            foreach (JsonElement npcElement in doc.RootElement.EnumerateArray())
            {
                NPC npc = NPCParser.ParseNPC(npcElement.GetRawText());
                npcs.Add(npc);
            }
        }

        return npcs;
    }


    
    // PHASE 1: Load base entities without references
    private List<Location> LoadLocations(string templatePath)
    {
        string locationsPath = Path.Combine(templatePath, "locations.json");
        if (!File.Exists(locationsPath))
        {
            Console.WriteLine("WARNING: locations.json not found");
            return new List<Location>();
        }
        
        try
        {
            string json = File.ReadAllText(locationsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var locationDTOs = JsonSerializer.Deserialize<List<LocationDTO>>(json, options);
            
            if (locationDTOs == null || !locationDTOs.Any())
            {
                Console.WriteLine("WARNING: No locations found in locations.json");
                return new List<Location>();
            }
            
            var locations = new List<Location>();
            foreach (var dto in locationDTOs)
            {
                try
                {
                    // Parse environmental properties
                    var envProps = new Dictionary<TimeBlocks, List<string>>();
                    if (dto.EnvironmentalProperties != null)
                    {
                        envProps[TimeBlocks.Morning] = dto.EnvironmentalProperties.Morning ?? new List<string>();
                        envProps[TimeBlocks.Afternoon] = dto.EnvironmentalProperties.Afternoon ?? new List<string>();
                        envProps[TimeBlocks.Evening] = dto.EnvironmentalProperties.Evening ?? new List<string>();
                        envProps[TimeBlocks.Night] = dto.EnvironmentalProperties.Night ?? new List<string>();
                    }
                    
                    // Parse available professions by time
                    var professionsByTime = new Dictionary<TimeBlocks, List<Professions>>();
                    foreach (var (timeStr, professionStrs) in dto.AvailableProfessionsByTime)
                    {
                        if (Enum.TryParse<TimeBlocks>(timeStr, out var timeBlock))
                        {
                            var professions = new List<Professions>();
                            foreach (var profStr in professionStrs)
                            {
                                if (Enum.TryParse<Professions>(profStr.Replace(" ", "_"), out var profession))
                                {
                                    professions.Add(profession);
                                }
                            }
                            professionsByTime[timeBlock] = professions;
                        }
                    }
                    
                    var location = _locationFactory.CreateLocation(
                        dto.Id,
                        dto.Name,
                        dto.Description,
                        dto.ConnectedTo,
                        dto.LocationSpots,
                        dto.DomainTags,
                        envProps,
                        professionsByTime);
                    
                    locations.Add(location);
                    Console.WriteLine($"Loaded location: {location.Id} - {location.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load location '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {locations.Count} locations");
            return locations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse locations.json: {ex.Message}");
            return new List<Location>();
        }
    }
    
    private List<Item> LoadItems(string templatePath)
    {
        string itemsPath = Path.Combine(templatePath, "items.json");
        if (!File.Exists(itemsPath))
        {
            Console.WriteLine("INFO: items.json not found");
            return new List<Item>();
        }
        
        try
        {
            string json = File.ReadAllText(itemsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var itemDTOs = JsonSerializer.Deserialize<List<ItemDTO>>(json, options);
            
            if (itemDTOs == null || !itemDTOs.Any())
            {
                Console.WriteLine("WARNING: No items found in items.json");
                return new List<Item>();
            }
            
            var items = new List<Item>();
            foreach (var dto in itemDTOs)
            {
                try
                {
                    // Parse categories
                    var categories = new List<ItemCategory>();
                    
                    // Parse from "categories" field
                    foreach (var catStr in dto.Categories ?? new List<string>())
                    {
                        if (Enum.TryParse<ItemCategory>(catStr.Replace(" ", "_"), out var cat))
                        {
                            categories.Add(cat);
                        }
                    }
                    
                    // Also parse from "itemCategories" for backwards compatibility
                    foreach (var catStr in dto.ItemCategories ?? new List<string>())
                    {
                        if (Enum.TryParse<ItemCategory>(catStr.Replace(" ", "_"), out var cat))
                        {
                            categories.Add(cat);
                        }
                    }
                    
                    
                    // Parse size category
                    if (!Enum.TryParse<SizeCategory>(dto.SizeCategory ?? "Small", out var sizeCategory))
                    {
                        sizeCategory = SizeCategory.Small;
                    }
                    
                    var item = _itemFactory.CreateItem(
                        dto.Id,
                        dto.Name,
                        dto.Weight,
                        dto.BuyPrice,
                        dto.SellPrice,
                        dto.InventorySlots,
                        sizeCategory,
                        categories,
                        dto.Description);
                    
                    items.Add(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load item '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {items.Count} items");
            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse items.json: {ex.Message}");
            return new List<Item>();
        }
    }
    
    private List<LocationSpot> LoadLocationSpots(string templatePath, GameWorld gameWorld)
    {
        string spotsPath = Path.Combine(templatePath, "location_spots.json");
        if (!File.Exists(spotsPath))
        {
            Console.WriteLine("WARNING: location_spots.json not found");
            return new List<LocationSpot>();
        }
        
        try
        {
            string json = File.ReadAllText(spotsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var spotDTOs = JsonSerializer.Deserialize<List<LocationSpotDTO>>(json, options);
            
            if (spotDTOs == null || !spotDTOs.Any())
            {
                Console.WriteLine("WARNING: No location spots found in location_spots.json");
                return new List<LocationSpot>();
            }
            
            var spots = new List<LocationSpot>();
            foreach (var dto in spotDTOs)
            {
                try
                {
                    // Parse type
                    if (!Enum.TryParse<LocationSpotTypes>(dto.Type, out var spotType))
                    {
                        spotType = LocationSpotTypes.FEATURE;
                    }
                    
                    // Parse time blocks
                    var timeBlocks = new List<TimeBlocks>();
                    foreach (var timeStr in dto.CurrentTimeBlocks ?? new List<string>())
                    {
                        if (Enum.TryParse<TimeBlocks>(timeStr, out var timeBlock))
                        {
                            timeBlocks.Add(timeBlock);
                        }
                    }
                    
                    var spot = _locationSpotFactory.CreateLocationSpotFromIds(
                        dto.Id,
                        dto.Name,
                        dto.LocationId,
                        spotType,
                        gameWorld.WorldState.locations,  // Pass available locations
                        dto.Description,
                        dto.InitialState,
                        timeBlocks,
                        dto.DomainTags);
                    
                    spots.Add(spot);
                    Console.WriteLine($"Loaded location spot: {spot.SpotID} at {spot.LocationId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load location spot '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {spots.Count} location spots");
            return spots;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse location_spots.json: {ex.Message}");
            return new List<LocationSpot>();
        }
    }
    
    private List<NPC> LoadNPCs(string templatePath, List<Location> availableLocations)
    {
        string npcsPath = Path.Combine(templatePath, "npcs.json");
        if (!File.Exists(npcsPath))
        {
            Console.WriteLine("INFO: npcs.json not found");
            return new List<NPC>();
        }
        
        try
        {
            string json = File.ReadAllText(npcsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var npcDTOs = JsonSerializer.Deserialize<List<NPCDTO>>(json, options);
            
            if (npcDTOs == null || !npcDTOs.Any())
            {
                Console.WriteLine("WARNING: No NPCs found in npcs.json");
                return new List<NPC>();
            }
            
            var npcs = new List<NPC>();
            foreach (var dto in npcDTOs)
            {
                try
                {
                    // Parse profession
                    if (!Enum.TryParse<Professions>(dto.Profession.Replace(" ", "_"), out var profession))
                    {
                        profession = Professions.Merchant; // Default profession if parsing fails
                    }
                    
                    // Parse schedule
                    if (!Enum.TryParse<Schedule>(dto.AvailabilitySchedule ?? "Standard", out var schedule))
                    {
                        schedule = Schedule.Business_Hours;
                    }
                    
                    // Parse services
                    var services = new List<ServiceTypes>();
                    foreach (var serviceStr in dto.Services ?? new List<string>())
                    {
                        if (Enum.TryParse<ServiceTypes>(serviceStr.Replace(" ", "_"), out var service))
                        {
                            services.Add(service);
                        }
                    }
                    
                    // Parse letter token types
                    var tokenTypes = new List<ConnectionType>();
                    foreach (var tokenTypeStr in dto.LetterTokenTypes ?? new List<string>())
                    {
                        if (Enum.TryParse<ConnectionType>(tokenTypeStr, out var parsed))
                        {
                            tokenTypes.Add(parsed);
                        }
                        else
                        {
                            Console.WriteLine($"WARNING: Unknown token type '{tokenTypeStr}' for NPC '{dto.Id}'");
                        }
                    }
                    
                    var npc = _npcFactory.CreateNPCFromIds(
                        dto.Id,
                        dto.Name,
                        dto.LocationId,
                        availableLocations,
                        profession,
                        dto.SpotId,
                        dto.Role,
                        dto.Description,
                        schedule,
                        services,
                        tokenTypes);
                    
                    npcs.Add(npc);
                    Console.WriteLine($"Loaded NPC: {npc.ID} - {npc.Name} at {npc.Location}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load NPC '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {npcs.Count} NPCs");
            return npcs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse npcs.json: {ex.Message}");
            return new List<NPC>();
        }
    }
    
    private List<RouteOption> LoadRoutes(string templatePath, List<Location> locations)
    {
        string routesPath = Path.Combine(templatePath, "routes.json");
        if (!File.Exists(routesPath))
        {
            Console.WriteLine("INFO: routes.json not found");
            return new List<RouteOption>();
        }
        
        try
        {
            string json = File.ReadAllText(routesPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var routeDTOs = JsonSerializer.Deserialize<List<RouteDTO>>(json, options);
            
            if (routeDTOs == null || !routeDTOs.Any())
            {
                Console.WriteLine("WARNING: No routes found in routes.json");
                return new List<RouteOption>();
            }
            
            var routes = new List<RouteOption>();
            foreach (var dto in routeDTOs)
            {
                try
                {
                    // Parse travel method
                    if (!Enum.TryParse<TravelMethods>(dto.Method, out var method))
                    {
                        method = TravelMethods.Walking;
                    }
                    
                    var route = _routeFactory.CreateRouteFromIds(
                        dto.Id,
                        dto.Name,
                        dto.Origin,
                        dto.Destination,
                        locations,
                        method,
                        dto.TravelTimeHours,
                        dto.BaseStaminaCost,
                        dto.BaseCoinCost,
                        dto.Description);
                    
                    // Set additional properties
                    route.IsDiscovered = dto.IsDiscovered;
                    route.MaxItemCapacity = dto.MaxItemCapacity;
                    
                    // Parse terrain categories
                    foreach (var terrainStr in dto.TerrainCategories ?? new List<string>())
                    {
                        if (Enum.TryParse<TerrainCategory>(terrainStr.Replace(" ", "_"), out var terrain))
                        {
                            route.TerrainCategories.Add(terrain);
                        }
                    }
                    
                    routes.Add(route);
                    Console.WriteLine($"Loaded route: {route.Id} from {route.Origin} to {route.Destination}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load route '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {routes.Count} routes");
            return routes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse routes.json: {ex.Message}");
            return new List<RouteOption>();
        }
    }
    
    private List<LetterTemplate> LoadLetterTemplates(string templatePath, List<NPC> availableNPCs)
    {
        string letterTemplatesPath = Path.Combine(templatePath, "letter_templates.json");
        if (!File.Exists(letterTemplatesPath))
        {
            Console.WriteLine("INFO: letter_templates.json not found");
            return new List<LetterTemplate>();
        }
        
        try
        {
            string json = File.ReadAllText(letterTemplatesPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var templateDTOs = JsonSerializer.Deserialize<List<LetterTemplateDTO>>(json, options);
            
            if (templateDTOs == null || !templateDTOs.Any())
            {
                Console.WriteLine("WARNING: No letter templates found in letter_templates.json");
                return new List<LetterTemplate>();
            }
            
            var templates = new List<LetterTemplate>();
            foreach (var dto in templateDTOs)
            {
                try
                {
                    // Parse token type
                    if (!Enum.TryParse<ConnectionType>(dto.TokenType, out var tokenType))
                    {
                        Console.WriteLine($"WARNING: Unknown token type '{dto.TokenType}' for template '{dto.Id}', skipping");
                        continue;
                    }
                    
                    // Parse letter category
                    var category = LetterCategory.Basic;
                    if (!string.IsNullOrEmpty(dto.Category))
                    {
                        if (!Enum.TryParse<LetterCategory>(dto.Category, out category))
                        {
                            Console.WriteLine($"WARNING: Unknown letter category '{dto.Category}' for template '{dto.Id}', defaulting to Basic");
                            category = LetterCategory.Basic;
                        }
                    }
                    
                    // Get minimum tokens required
                    int minTokensRequired = dto.MinTokensRequired ?? 3;
                    
                    // Parse letter size
                    var size = SizeCategory.Medium;
                    if (!string.IsNullOrEmpty(dto.Size))
                    {
                        if (!Enum.TryParse<SizeCategory>(dto.Size, out size))
                        {
                            Console.WriteLine($"WARNING: Unknown letter size '{dto.Size}' for template '{dto.Id}', defaulting to Medium");
                            size = SizeCategory.Medium;
                        }
                    }
                    
                    // Parse physical properties
                    var physicalProperties = LetterPhysicalProperties.None;
                    if (dto.PhysicalProperties != null && dto.PhysicalProperties.Any())
                    {
                        foreach (var prop in dto.PhysicalProperties)
                        {
                            if (Enum.TryParse<LetterPhysicalProperties>(prop, out var propEnum))
                            {
                                physicalProperties |= propEnum;
                            }
                            else
                            {
                                Console.WriteLine($"WARNING: Unknown physical property '{prop}' for template '{dto.Id}'");
                            }
                        }
                    }
                    
                    // Parse required equipment
                    ItemCategory? requiredEquipment = null;
                    if (!string.IsNullOrEmpty(dto.RequiredEquipment))
                    {
                        if (Enum.TryParse<ItemCategory>(dto.RequiredEquipment, out var equipment))
                        {
                            requiredEquipment = equipment;
                        }
                        else
                        {
                            Console.WriteLine($"WARNING: Unknown equipment category '{dto.RequiredEquipment}' for template '{dto.Id}'");
                        }
                    }
                    
                    var template = _letterTemplateFactory.CreateLetterTemplateFromIds(
                        dto.Id,
                        dto.Description,
                        tokenType,
                        dto.MinDeadline,
                        dto.MaxDeadline,
                        dto.MinPayment,
                        dto.MaxPayment,
                        category,
                        minTokensRequired,
                        dto.PossibleSenders,
                        dto.PossibleRecipients,
                        availableNPCs,
                        dto.UnlocksLetterIds,
                        dto.IsChainLetter,
                        size,
                        physicalProperties,
                        requiredEquipment);
                    
                    templates.Add(template);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load letter template '{dto.Id}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {templates.Count} letter templates");
            return templates;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse letter_templates.json: {ex.Message}");
            return new List<LetterTemplate>();
        }
    }
    
    private List<StandingObligation> LoadStandingObligations(string templatePath, List<NPC> availableNPCs)
    {
        string obligationsPath = Path.Combine(templatePath, "standing_obligations.json");
        if (!File.Exists(obligationsPath))
        {
            Console.WriteLine("INFO: standing_obligations.json not found");
            return new List<StandingObligation>();
        }
        
        try
        {
            string json = File.ReadAllText(obligationsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var obligationDTOs = JsonSerializer.Deserialize<List<StandingObligationDTO>>(json, options);
            
            if (obligationDTOs == null || !obligationDTOs.Any())
            {
                Console.WriteLine("WARNING: No standing obligations found in standing_obligations.json");
                return new List<StandingObligation>();
            }
            
            var obligations = new List<StandingObligation>();
            foreach (var dto in obligationDTOs)
            {
                try
                {
                    var obligation = _standingObligationFactory.CreateStandingObligationFromDTO(dto, availableNPCs);
                    obligations.Add(obligation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to load standing obligation '{dto.ID}': {ex.Message}");
                }
            }
            
            Console.WriteLine($"Loaded {obligations.Count} standing obligations");
            return obligations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to parse standing_obligations.json: {ex.Message}");
            return new List<StandingObligation>();
        }
    }
    
    // Action system removed - location actions handled by LocationActionManager
}