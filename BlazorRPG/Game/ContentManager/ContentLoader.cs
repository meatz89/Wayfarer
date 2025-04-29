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
        if (!Directory.Exists(locationsPath))
            return locations;

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
        if (!Directory.Exists(spotsPath))
            return spots;

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
        string actionsPath = Path.Combine(_contentDirectory, "Actions");
        if (!Directory.Exists(actionsPath))
            return actions;

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

    public GameState LoadGame()
    {
        try
        {
            string savePath = Path.Combine(_contentDirectory, _saveFolder);
            GameState gameState = new GameState();

            // Check if save files exist
            if (HasSaveFiles(savePath))
            {
                // Load content from save files
                List<Location> locations = GameStateSerializer.DeserializeLocations(
                    File.ReadAllText(Path.Combine(savePath, "locations.json")));

                List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
                    File.ReadAllText(Path.Combine(savePath, "locationSpots.json")));

                List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
                    File.ReadAllText(Path.Combine(savePath, "actions.json")));

                // Load game state using the loaded content
                gameState = GameStateSerializer.DeserializeGameState(
                    File.ReadAllText(Path.Combine(savePath, "gameState.json")),
                    locations, spots, actions);
            }
            else if(HasTemplateFiles())
            {
                return LoadGameFromTemplates();
            }
            else
            {
                return CreateNewGameState();
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

        // Load content from save files
        List<Location> locations = GameStateSerializer.DeserializeLocations(
            File.ReadAllText(Path.Combine(templatePath, "locations.json")));

        List<LocationSpot> spots = GameStateSerializer.DeserializeLocationSpots(
            File.ReadAllText(Path.Combine(templatePath, "locationSpots.json")));

        List<ActionDefinition> actions = GameStateSerializer.DeserializeActions(
            File.ReadAllText(Path.Combine(templatePath, "actions.json")));

        // Load game state using the loaded content
        var gameState = GameStateSerializer.DeserializeGameState(
            File.ReadAllText(Path.Combine(templatePath, "gameState.json")),
            locations, spots, actions);

        return gameState;
    }

    public void SaveGame(GameState gameState)
    {
        try
        {
            if (!HasSaveFiles(_saveFolder) && HasTemplateFiles())
            {
                CopyTemplateToSave();
            }

            string savePath = Path.Combine(_contentDirectory, _saveFolder);

            // Serialize and save all content
            File.WriteAllText(
                Path.Combine(savePath, "gameState.json"),
                GameStateSerializer.SerializeGameState(gameState));

            File.WriteAllText(
                Path.Combine(savePath, "locations.json"),
                GameStateSerializer.SerializeLocations(gameState.WorldState.locations));

            File.WriteAllText(
                Path.Combine(savePath, "locationSpots.json"),
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

    private bool HasSaveFiles(string savePath)
    {
        return File.Exists(Path.Combine(savePath, "gameState.json")) &&
               File.Exists(Path.Combine(savePath, "locations.json")) &&
               File.Exists(Path.Combine(savePath, "locationSpots.json")) &&
               File.Exists(Path.Combine(savePath, "actions.json"));
    }

    private bool HasTemplateFiles()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");
        return Directory.Exists(templatePath) &&
               File.Exists(Path.Combine(templatePath, "gameState.json")) &&
               File.Exists(Path.Combine(templatePath, "locations.json")) &&
               File.Exists(Path.Combine(templatePath, "locationSpots.json")) &&
               File.Exists(Path.Combine(templatePath, "actions.json"));
    }

    private void CopyTemplateToSave()
    {
        string templatePath = Path.Combine(_contentDirectory, "Templates");
        string savePath = Path.Combine(_contentDirectory, _saveFolder);

        File.Copy(Path.Combine(templatePath, "gameState.json"), Path.Combine(savePath, "gameState.json"), true);
        File.Copy(Path.Combine(templatePath, "locations.json"), Path.Combine(savePath, "locations.json"), true);
        File.Copy(Path.Combine(templatePath, "locationSpots.json"), Path.Combine(savePath, "locationSpots.json"), true);
        File.Copy(Path.Combine(templatePath, "actions.json"), Path.Combine(savePath, "actions.json"), true);
    }

    private GameState CreateNewGameState()
    {
        GameState gameState = new GameState();

        // Load fresh content from the content directory
        List<Location> locations = LoadLocations();
        List<LocationSpot> spots = LoadLocationSpots();
        List<ActionDefinition> actions = LoadActions();

        // Add content to game state
        gameState.WorldState.locations.Clear();
        gameState.WorldState.locations.AddRange(locations);

        gameState.WorldState.locationSpots.Clear();
        gameState.WorldState.locationSpots.AddRange(spots);

        gameState.WorldState.actions.Clear();
        gameState.WorldState.actions.AddRange(actions);

        return gameState;
    }
}