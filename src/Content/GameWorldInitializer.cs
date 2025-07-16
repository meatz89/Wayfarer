using System.Text.Json;
using Wayfarer.Content;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;

public class GameWorldInitializer
{
    private string _contentDirectory;

    public GameWorldInitializer(string contentDirectory)
    {
        _contentDirectory = contentDirectory;
    }

    public GameWorld LoadGame()
    {
        GameWorld gameWorld = CreateInitialGameWorld();
        return LoadGameFromTemplates();
    }

    private GameWorld LoadGameFromTemplates()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");

        // Load content from template files
        List<Location> locations = GameWorldSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(templatePath, "locations.json")));

        List<LocationSpot> spots = GameWorldSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(templatePath, "location_spots.json")));

        // Connect locations to spots
        ConnectLocationsToSpots(locations, spots);

        // Load route options
        List<RouteOption> routes = new List<RouteOption>();
        string routesFilePath = Path.Combine(templatePath, "routes.json");
        if (File.Exists(routesFilePath))
        {
            routes = GameWorldSerializer.DeserializeRouteOptions(
                File.ReadAllText(routesFilePath));
        }

        // Connect routes to locations
        ConnectRoutesToLocations(locations, routes);

        // Load items
        List<Item> items = new List<Item>();
        string itemsFilePath = Path.Combine(templatePath, "items.json");
        if (File.Exists(itemsFilePath))
        {
            items = GameWorldSerializer.DeserializeItems(
                File.ReadAllText(itemsFilePath));
        }

        // Contract system removed - using letter system instead

        List<ActionDefinition> actions = GameWorldSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(templatePath, "actions.json")));

        // Load cards if available
        List<SkillCard> cards = new List<SkillCard>();
        string cardsFilePath = Path.Combine(templatePath, "cards.json");
        if (File.Exists(cardsFilePath))
        {
            // Add card deserialization logic here if needed
        }

        // Load game state using the loaded content
        GameWorld gameWorld = GameWorldSerializer.DeserializeGameWorld(
            File.ReadAllText(Path.Combine(templatePath, "gameWorld.json")),
            locations, spots, actions, cards);

        // Add items to the game world - items are now loaded from JSON
        if (gameWorld.WorldState.Items == null)
        {
            gameWorld.WorldState.Items = new List<Item>();
        }

        // Only add items if they were successfully loaded from JSON
        if (items.Any())
        {
            gameWorld.WorldState.Items.AddRange(items);
        }
        else
        {
            Console.WriteLine("WARNING: No items loaded from JSON templates. Check items.json file.");
        }

        // Load NPCs
        List<NPC> npcs = new List<NPC>();
        string npcsFilePath = Path.Combine(templatePath, "npcs.json");
        if (File.Exists(npcsFilePath))
        {
            try
            {
                npcs = ParseNPCArray(File.ReadAllText(npcsFilePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to load NPCs from JSON: {ex.Message}");
            }
        }

        // Load information
        List<Information> informations = new List<Information>();
        string informationsFilePath = Path.Combine(templatePath, "informations.json");
        if (File.Exists(informationsFilePath))
        {
            try
            {
                informations = InformationParser.ParseInformationArray(
                    File.ReadAllText(informationsFilePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to load informations from JSON: {ex.Message}");
            }
        }

        // Load letter templates
        List<LetterTemplate> letterTemplates = new List<LetterTemplate>();
        string letterTemplatesFilePath = Path.Combine(templatePath, "letter_templates.json");
        if (File.Exists(letterTemplatesFilePath))
        {
            try
            {
                letterTemplates = ParseLetterTemplateArray(File.ReadAllText(letterTemplatesFilePath));
                Console.WriteLine($"Loaded {letterTemplates.Count} letter templates from JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to load letter templates from JSON: {ex.Message}");
            }
        }

        // Load standing obligations
        List<StandingObligation> standingObligations = new List<StandingObligation>();
        string standingObligationsFilePath = Path.Combine(templatePath, "standing_obligations.json");
        if (File.Exists(standingObligationsFilePath))
        {
            try
            {
                standingObligations = ParseStandingObligationArray(File.ReadAllText(standingObligationsFilePath));
                Console.WriteLine($"Loaded {standingObligations.Count} standing obligation templates from JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to load standing obligations from JSON: {ex.Message}");
            }
        }

        // Add NPCs to the game world
        if (npcs.Any())
        {
            foreach (NPC npc in npcs)
            {
                gameWorld.WorldState.AddCharacter(npc);
            }
            Console.WriteLine($"Loaded {npcs.Count} NPCs from JSON templates.");
        }
        else
        {
            Console.WriteLine("INFO: No NPCs loaded from JSON templates. Create npcs.json file to add NPC content.");
        }

        // Connect NPCs to their location spots
        ConnectNPCsToLocationSpots(npcs, gameWorld.WorldState.locationSpots);

        // Add information to the game world
        if (gameWorld.WorldState.Informations == null)
        {
            gameWorld.WorldState.Informations = new List<Information>();
        }

        if (informations.Any())
        {
            gameWorld.WorldState.Informations.AddRange(informations);
            Console.WriteLine($"Loaded {informations.Count} information entries from JSON templates.");
        }
        else
        {
            Console.WriteLine("INFO: No information loaded from JSON templates. Create informations.json file to add information content.");
        }

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
                        TimeBlockCost = 1,
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

        List<ActionDefinition> actions = new List<ActionDefinition>();
        // Contract system removed - using letter system instead

        List<SkillCard> cards = new List<SkillCard>();

        // Add content to game state
        gameWorld.WorldState.locations.Clear();
        gameWorld.WorldState.locations.AddRange(locations);

        gameWorld.WorldState.locationSpots.Clear();
        gameWorld.WorldState.locationSpots.AddRange(spots);

        gameWorld.WorldState.actions.Clear();
        gameWorld.WorldState.actions.AddRange(actions);
        // Contract system removed - using letter system instead

        // Add cards to world state if applicable
        if (gameWorld.WorldState.AllCards != null)
        {
            gameWorld.WorldState.AllCards.Clear();
            gameWorld.WorldState.AllCards.AddRange(cards);
        }

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

    private List<LetterTemplate> ParseLetterTemplateArray(string letterTemplatesJson)
    {
        List<LetterTemplate> templates = new List<LetterTemplate>();

        using (JsonDocument doc = JsonDocument.Parse(letterTemplatesJson))
        {
            foreach (JsonElement templateElement in doc.RootElement.EnumerateArray())
            {
                LetterTemplate template = LetterTemplateParser.ParseLetterTemplate(templateElement.GetRawText());
                templates.Add(template);
            }
        }

        return templates;
    }

    private List<StandingObligation> ParseStandingObligationArray(string obligationsJson)
    {
        List<StandingObligation> obligations = new List<StandingObligation>();

        using (JsonDocument doc = JsonDocument.Parse(obligationsJson))
        {
            foreach (JsonElement obligationElement in doc.RootElement.EnumerateArray())
            {
                StandingObligation obligation = ParseStandingObligation(obligationElement);
                obligations.Add(obligation);
            }
        }

        return obligations;
    }

    private StandingObligation ParseStandingObligation(JsonElement element)
    {
        var obligation = new StandingObligation();

        if (element.TryGetProperty("ID", out var idElement))
            obligation.ID = idElement.GetString() ?? "";

        if (element.TryGetProperty("Name", out var nameElement))
            obligation.Name = nameElement.GetString() ?? "";

        if (element.TryGetProperty("Description", out var descElement))
            obligation.Description = descElement.GetString() ?? "";

        if (element.TryGetProperty("Source", out var sourceElement))
            obligation.Source = sourceElement.GetString() ?? "";

        if (element.TryGetProperty("RelatedTokenType", out var tokenElement) && 
            !tokenElement.ValueKind.Equals(JsonValueKind.Null))
        {
            if (Enum.TryParse<ConnectionType>(tokenElement.GetString(), out var tokenType))
                obligation.RelatedTokenType = tokenType;
        }

        if (element.TryGetProperty("BenefitEffects", out var benefitsElement))
        {
            foreach (var benefitElement in benefitsElement.EnumerateArray())
            {
                if (Enum.TryParse<ObligationEffect>(benefitElement.GetString(), out var effect))
                    obligation.BenefitEffects.Add(effect);
            }
        }

        if (element.TryGetProperty("ConstraintEffects", out var constraintsElement))
        {
            foreach (var constraintElement in constraintsElement.EnumerateArray())
            {
                if (Enum.TryParse<ObligationEffect>(constraintElement.GetString(), out var effect))
                    obligation.ConstraintEffects.Add(effect);
            }
        }

        return obligation;
    }
}