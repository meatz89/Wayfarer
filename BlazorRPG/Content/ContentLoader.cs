public class ContentLoader
{
    private string _contentDirectory;
    private string _saveFolder = "Saves";

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

    public GameWorld LoadGame()
    {
        string savePath = Path.Combine(_contentDirectory, _saveFolder);
        GameWorld gameWorld = CreateNewGameWorld();

        bool shouldLoad = false;

        // Check if save files exist
        if (Directory.Exists(savePath) && shouldLoad)
        {
            gameWorld = LoadGameFromSaveFile(savePath);
        }
        else
        {
            return LoadGameFromTemplates();
        }

        return gameWorld;
    }

    private static GameWorld LoadGameFromSaveFile(string savePath)
    {
        GameWorld gameWorld;
        // Load content from save files
        List<Location> locations = GameWorldSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(savePath, "locations.json")));

        List<LocationSpot> spots = GameWorldSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(savePath, "locationSpots.json")));

        List<ActionDefinition> actions = GameWorldSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(savePath, "actions.json")));

        List<OpportunityDefinition> opportunities = GameWorldSerializer.DeserializeOpportunities(
            File.ReadAllText(Path.Combine(savePath, "Opportunities.json")));

        // Load cards if available
        List<SkillCard> cards = new List<SkillCard>();
        string cardsFilePath = Path.Combine(savePath, "cards.json");

        // Load game state using the loaded content
        gameWorld = GameWorldSerializer.DeserializeGameWorld(
            File.ReadAllText(Path.Combine(savePath, "gameWorld.json")),
            locations, spots, actions, cards);
        return gameWorld;
    }

    private GameWorld LoadGameFromTemplates()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");

        // Load content from template files
        List<Location> locations = GameWorldSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(templatePath, "locations.json")));

        List<LocationSpot> spots = GameWorldSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(templatePath, "locationSpots.json")));

        ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = GameWorldSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(templatePath, "actions.json")));

        // Load cards if available
        List<SkillCard> cards = new List<SkillCard>();
        string cardsFilePath = Path.Combine(templatePath, "cards.json");

        // Load game state using the loaded content
        GameWorld gameWorld = GameWorldSerializer.DeserializeGameWorld(
            File.ReadAllText(Path.Combine(templatePath, "gameWorld.json")),
            locations, spots, actions, cards);

        return gameWorld;
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
                Path.Combine(savePath, "location_Spots.json"),
                GameWorldSerializer.SerializeLocationSpots(gameWorld.WorldState.locationSpots));

            File.WriteAllText(
                Path.Combine(savePath, "actions.json"),
                GameWorldSerializer.SerializeActions(gameWorld.WorldState.actions));

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
        File.Copy(Path.Combine(templatePath, "basic_Opportunities.json"), Path.Combine(savePath, "Opportunities.json"), true);

        // Copy cards.json if it exists
        string templateCardsPath = Path.Combine(templatePath, "cards.json");
        if (File.Exists(templateCardsPath))
        {
            File.Copy(templateCardsPath, Path.Combine(savePath, "cards.json"), true);
        }
    }

    private GameWorld CreateNewGameWorld()
    {
        GameWorld gameWorld = new GameWorld();

        // Load fresh content from the content directory
        List<Location> locations = new List<Location>();
        List<LocationSpot> spots = new List<LocationSpot>();
        locations = ConnectLocationsToSpots(locations, spots);

        List<ActionDefinition> actions = new List<ActionDefinition>();
        List<OpportunityDefinition> comissions = new List<OpportunityDefinition>();

        List<SkillCard> cards = new List<SkillCard>();

        // Add content to game state
        gameWorld.WorldState.locations.Clear();
        gameWorld.WorldState.locations.AddRange(locations);

        gameWorld.WorldState.locationSpots.Clear();
        gameWorld.WorldState.locationSpots.AddRange(spots);

        gameWorld.WorldState.actions.Clear();
        gameWorld.WorldState.actions.AddRange(actions);
        gameWorld.WorldState.Opportunities.AddRange(comissions);

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