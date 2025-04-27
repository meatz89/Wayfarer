public class ContentLoader
{
    private readonly string _contentDirectory;

    public ContentLoader(string contentDirectory)
    {
        _contentDirectory = contentDirectory;
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
                Location location = ContentParser.ParseLocation(json);
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
                LocationSpot spot = ContentParser.ParseLocationSpot(json);
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
}