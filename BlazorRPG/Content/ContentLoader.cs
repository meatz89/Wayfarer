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

    public GameState LoadGame()
    {
        string savePath = Path.Combine(_contentDirectory, _saveFolder);
        GameState gameState = CreateNewGameState();

        bool shouldLoad = false;

        // Check if save files exist
        if (Directory.Exists(savePath) && shouldLoad)
        {
            gameState = LoadGameFromSaveFile(savePath);
        }
        else
        {
            return LoadGameFromTemplates();
        }

        return gameState;
    }

    private static GameState LoadGameFromSaveFile(string savePath)
    {
        GameState gameState;
        // Load content from save files
        List<Location> locations = GameStateSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(savePath, "locations.json")));

        List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(savePath, "locationSpots.json")));

        List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(savePath, "actions.json")));

        List<CommissionDefinition> commissions = GameStateSerializer.DeserializeCommissions(
            File.ReadAllText(Path.Combine(savePath, "commissions.json")));

        // Load cards if available
        List<SkillCard> cards = new List<SkillCard>();
        string cardsFilePath = Path.Combine(savePath, "cards.json");

        // Load game state using the loaded content
        gameState = GameStateSerializer.DeserializeGameState(
            File.ReadAllText(Path.Combine(savePath, "gameState.json")),
            locations, spots, actions, commissions, cards);
        return gameState;
    }

    private GameState LoadGameFromTemplates()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");

        // Load content from template files
        List<Location> locations = GameStateSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(templatePath, "locations.json")));

        List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(templatePath, "locationSpots.json")));

        ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(templatePath, "actions.json")));

        List<CommissionDefinition> commissions = GameStateSerializer.DeserializeCommissions(
            File.ReadAllText(Path.Combine(templatePath, "commissions.json")));

        // Load cards if available
        List<SkillCard> cards = new List<SkillCard>();
        string cardsFilePath = Path.Combine(templatePath, "cards.json");

        // Load game state using the loaded content
        GameState gameState = GameStateSerializer.DeserializeGameState(
            File.ReadAllText(Path.Combine(templatePath, "gameState.json")),
            locations, spots, actions, commissions, cards);

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
        List<Location> locations = new List<Location>();
        List<LocationSpot> spots = new List<LocationSpot>();
        locations = ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = new List<ActionDefinition>();
        List<CommissionDefinition> comissions = new List<CommissionDefinition>();

        List<SkillCard> cards = new List<SkillCard>();

        // Add content to game state
        gameState.WorldState.locations.Clear();
        gameState.WorldState.locations.AddRange(locations);

        gameState.WorldState.locationSpots.Clear();
        gameState.WorldState.locationSpots.AddRange(spots);

        gameState.WorldState.actions.Clear();
        gameState.WorldState.actions.AddRange(actions);
        gameState.WorldState.commissions.AddRange(comissions);

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