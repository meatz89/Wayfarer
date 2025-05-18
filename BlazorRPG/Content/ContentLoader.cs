public class ContentLoader
{
    private readonly string _contentDirectory;
    private readonly string _saveFolder = "Saves";

    public ContentLoader(string contentDirectory)
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

    public List<Location> LoadLocations()
    {
        List<Location> locations = new List<Location>();
        string locationsPath = Path.Combine(_contentDirectory, "Locations");

        foreach (string filePath in Directory.GetFiles(locationsPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                Location location = LocationParser.ParseLocation(json);
                locations.Add(location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading location from {filePath}: {ex.Message}");
            }
        }
        return locations;
    }

    public List<LocationSpot> LoadLocationSpots()
    {
        List<LocationSpot> spots = new List<LocationSpot>();
        string spotsPath = Path.Combine(_contentDirectory, "LocationSpots");

        foreach (string filePath in Directory.GetFiles(spotsPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                LocationSpot spot = LocationParser.ParseLocationSpot(json);
                spots.Add(spot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading location spot from {filePath}: {ex.Message}");
            }
        }

        return spots;
    }

    public List<ActionDefinition> LoadActions()
    {
        List<ActionDefinition> actions = new List<ActionDefinition>();
        string actionsPath = Path.Combine(_contentDirectory, "basic_actions");

        foreach (string filePath in Directory.GetFiles(actionsPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                ActionDefinition action = ActionParser.ParseAction(json);
                actions.Add(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading action from {filePath}: {ex.Message}");
            }
        }
        return actions;
    }

    public List<CommissionDefinition> LoadCommissions()
    {
        List<CommissionDefinition> commissions = new List<CommissionDefinition>();
        string commissionsPath = Path.Combine(_contentDirectory, "basic_commissions");

        foreach (string filePath in Directory.GetFiles(commissionsPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                CommissionDefinition action = CommissionParser.ParseCommission(json);
                commissions.Add(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading action from {filePath}: {ex.Message}");
            }
        }

        return commissions;
    }

    public List<CardDefinition> LoadCards()
    {
        List<CardDefinition> cards = new List<CardDefinition>();
        string cardsPath = Path.Combine(_contentDirectory, "Cards");

        foreach (string filePath in Directory.GetFiles(cardsPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                CardDefinition card = CardParser.ParseCard(json);
                cards.Add(card);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading card from {filePath}: {ex.Message}");
            }
        }
        return cards;
    }

    public GameState LoadGame()
    {
        try
        {
            string savePath = Path.Combine(_contentDirectory, _saveFolder);
            GameState gameState = new GameState();

            bool shouldLoad = false;

            // Check if save files exist
            if (Directory.Exists(savePath) && shouldLoad)
            {
                // Load content from save files
                List<Location> locations = GameStateSerializer.DeserializeLocations(
                    File.ReadAllText(Path.Combine(savePath, "locations.json")));

                List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
                    File.ReadAllText(Path.Combine(savePath, "locationSpots.json")));

                List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
                    File.ReadAllText(Path.Combine(savePath, "actions.json")));

                // Load cards if available
                List<CardDefinition> cards = new List<CardDefinition>();
                string cardsFilePath = Path.Combine(savePath, "cards.json");
                if (File.Exists(cardsFilePath))
                {
                    cards = GameStateSerializer.DeserializeCards(
                        File.ReadAllText(cardsFilePath));
                }

                // Load game state using the loaded content
                gameState = GameStateSerializer.DeserializeGameState(
                    File.ReadAllText(Path.Combine(savePath, "gameState.json")),
                    locations, spots, actions, cards);
            }
            else
            {
                return LoadGameFromTemplates();
            }

            return gameState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game: {ex.Message}");
            return CreateNewGameState();
        }
    }

    private GameState LoadGameFromTemplates()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");

        // Load content from template files
        List<Location> locations = GameStateSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(templatePath, "locations.json")));

        List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(templatePath, "location_Spots.json")));

        ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(templatePath, "basic_actions.json")));

        List<CommissionDefinition> commissions = new List<CommissionDefinition>();
        File.ReadAllText(Path.Combine(templatePath, "basic_commissions.json"));

        // Load cards if available
        List<CardDefinition> cards = new List<CardDefinition>();
        string cardsFilePath = Path.Combine(templatePath, "cards.json");
        if (File.Exists(cardsFilePath))
        {
            cards = GameStateSerializer.DeserializeCards(
                File.ReadAllText(cardsFilePath));
        }

        // Load game state using the loaded content
        GameState gameState = GameStateSerializer.DeserializeGameState(
            File.ReadAllText(Path.Combine(templatePath, "gameState.json")),
            locations, spots, actions, cards);

        return gameState;
    }

    public void SaveGame(GameState gameState)
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
                Path.Combine(savePath, "gameState.json"),
                GameStateSerializer.SerializeGameState(gameState));

            File.WriteAllText(
                Path.Combine(savePath, "locations.json"),
                GameStateSerializer.SerializeLocations(gameState.WorldState.locations));

            File.WriteAllText(
                Path.Combine(savePath, "location_Spots.json"),
                GameStateSerializer.SerializeLocationSpots(gameState.WorldState.locationSpots));

            File.WriteAllText(
                Path.Combine(savePath, "actions.json"),
                GameStateSerializer.SerializeActions(gameState.WorldState.actions));

            // Save cards if they exist in world state
            if (gameState.WorldState.AllCards != null && gameState.WorldState.AllCards.Count > 0)
            {
                File.WriteAllText(
                    Path.Combine(savePath, "cards.json"),
                    GameStateSerializer.SerializeCards(gameState.WorldState.AllCards));
            }

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

        File.Copy(Path.Combine(templatePath, "gameState.json"), Path.Combine(savePath, "gameState.json"), true);
        File.Copy(Path.Combine(templatePath, "locations.json"), Path.Combine(savePath, "locations.json"), true);
        File.Copy(Path.Combine(templatePath, "location_spots.json"), Path.Combine(savePath, "locationSpots.json"), true);
        File.Copy(Path.Combine(templatePath, "basic_actions.json"), Path.Combine(savePath, "actions.json"), true);
        File.Copy(Path.Combine(templatePath, "basic_commissions.json"), Path.Combine(savePath, "commissions.json"), true);

        // Copy cards.json if it exists
        string templateCardsPath = Path.Combine(templatePath, "cards.json");
        if (File.Exists(templateCardsPath))
        {
            File.Copy(templateCardsPath, Path.Combine(savePath, "cards.json"), true);
        }
    }

    private GameState CreateNewGameState()
    {
        GameState gameState = new GameState();

        // Load fresh content from the content directory
        List<Location> locations = LoadLocations();
        List<LocationSpot> spots = LoadLocationSpots();
        locations = ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = LoadActions();
        List<CommissionDefinition> comissions = LoadCommissions();

        List<CardDefinition> cards = LoadCards();

        // Add content to game state
        gameState.WorldState.locations.Clear();
        gameState.WorldState.locations.AddRange(locations);

        gameState.WorldState.locationSpots.Clear();
        gameState.WorldState.locationSpots.AddRange(spots);

        gameState.WorldState.actions.Clear();
        gameState.WorldState.actions.AddRange(actions);
        gameState.WorldState.comissions.AddRange(comissions);

        // Add cards to world state if applicable
        if (gameState.WorldState.AllCards != null)
        {
            gameState.WorldState.AllCards.Clear();
            gameState.WorldState.AllCards.AddRange(cards);
        }

        return gameState;
    }

    private List<Location> ConnectLocationsToSpots(List<Location> locations, List<LocationSpot> spots)
    {
        foreach (Location location in locations)
        {
            foreach (string locSpotId in location.LocationSpotIds)
            {
                LocationSpot? spot = spots.FirstOrDefault(s => s.Id == locSpotId);
                if (spot != null)
                {
                    location.LocationSpots.Add(spot);
                    spot.LocationId = location.Id;
                }
            }
        }

        return locations;
    }
}