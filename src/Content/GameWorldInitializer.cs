public class GameWorldInitializer
{
    private string _contentDirectory;
    private string _saveFolder = "Saves";

    public GameWorldInitializer(string contentDirectory)
    {
        _contentDirectory = contentDirectory;
        EnsureSaveDirectoryExists();
    }

    private void EnsureSaveDirectoryExists()
    {
        string savePath = Path.Combine(_contentDirectory, _saveFolder);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    public GameWorld LoadGame()
    {
        string savePath = Path.Combine(_contentDirectory, _saveFolder);
        GameWorld gameWorld = CreateInitialGameWorld();

        bool shouldLoad = false;

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

        // Load contracts
        List<Contract> contracts = new List<Contract>();
        string contractsFilePath = Path.Combine(templatePath, "contracts.json");
        if (File.Exists(contractsFilePath))
        {
            contracts = GameWorldSerializer.DeserializeContracts(
                File.ReadAllText(contractsFilePath));

            contracts.Add(
                new Contract
                {
                    Id = "deliver_tools",
                    Description = "Deliver tools to the town carpenter",
                    RequiredItems = new List<string> { "tools" },
                    DestinationLocation = "town_square",
                    StartDay = 1,
                    DueDay = 3,
                    Payment = 15,
                    FailurePenalty = "Loss of reputation with craftsmen"
                }
            );
        }

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

        // Add contracts to the game world
        GameWorld.AllContracts = contracts;

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

    public void SaveGame(GameWorld gameWorld)
    {
        try
        {
            string savePath = Path.Combine(_contentDirectory, _saveFolder);
            if (!Directory.Exists(savePath))
            {
                CopyTemplateToSave();
            }

            // Serialize and save all content
            File.WriteAllText(
                Path.Combine(savePath, "gameWorld.json"),
                GameWorldSerializer.SerializeGameWorld(gameWorld));

            File.WriteAllText(
                Path.Combine(savePath, "locations.json"),
                GameWorldSerializer.SerializeLocations(gameWorld.WorldState.locations));

            File.WriteAllText(
                Path.Combine(savePath, "location_spots.json"),
                GameWorldSerializer.SerializeLocationSpots(gameWorld.WorldState.locationSpots));

            File.WriteAllText(
                Path.Combine(savePath, "actions.json"),
                GameWorldSerializer.SerializeActions(gameWorld.WorldState.actions));

            // Save the new content types
            File.WriteAllText(
                Path.Combine(savePath, "items.json"),
                GameWorldSerializer.SerializeItems(gameWorld.WorldState.Items));

            File.WriteAllText(
                Path.Combine(savePath, "routes.json"),
                GameWorldSerializer.SerializeRouteOptions(gameWorld.DiscoveredRoutes));

            File.WriteAllText(
                Path.Combine(savePath, "contracts.json"),
                GameWorldSerializer.SerializeContracts(GameWorld.AllContracts));

            Console.WriteLine("Game saved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving game: {ex.Message}");
            throw; // Rethrow so caller can handle it
        }
    }

    private void CopyTemplateToSave()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");
        string savePath = Path.Combine(_contentDirectory, _saveFolder);

        File.Copy(Path.Combine(templatePath, "gameWorld.json"), Path.Combine(savePath, "gameWorld.json"), true);
        File.Copy(Path.Combine(templatePath, "locations.json"), Path.Combine(savePath, "locations.json"), true);
        File.Copy(Path.Combine(templatePath, "location_spots.json"), Path.Combine(savePath, "locationSpots.json"), true);
        File.Copy(Path.Combine(templatePath, "basic_actions.json"), Path.Combine(savePath, "actions.json"), true);
        File.Copy(Path.Combine(templatePath, "basic_Contracts.json"), Path.Combine(savePath, "Contracts.json"), true);

        // Copy cards.json if it exists
        string templateCardsPath = Path.Combine(templatePath, "cards.json");
        if (File.Exists(templateCardsPath))
        {
            File.Copy(templateCardsPath, Path.Combine(savePath, "cards.json"), true);
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
        List<Contract> comissions = new List<Contract>();

        List<SkillCard> cards = new List<SkillCard>();

        // Add content to game state
        gameWorld.WorldState.locations.Clear();
        gameWorld.WorldState.locations.AddRange(locations);

        gameWorld.WorldState.locationSpots.Clear();
        gameWorld.WorldState.locationSpots.AddRange(spots);

        gameWorld.WorldState.actions.Clear();
        gameWorld.WorldState.actions.AddRange(actions);
        gameWorld.WorldState.Contracts.AddRange(comissions);

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
}