public class LocationSystem
{
    private readonly GameState gameState;

    public LocationSystem(GameState gameState, GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        var allLocations = contentProvider.GetLocations();


        var location = InitialLocationGenerator.GenerateNewLocation("");
        gameState.WorldState.AddLocation(location.Name, location);
    }

    public List<Location> GetAllLocations()
    {
        return gameState.WorldState.GetLocations();
    }

    public List<string> GetTravelLocations(string currentLocation)
    {
        Location location = GetLocation(currentLocation);
        return location.ConnectedLocationIds;
    }

    public Location GetLocation(string locationName)
    {
        Location location = GetAllLocations().FirstOrDefault(x => x.Name == locationName);
        return location;
    }

    public List<LocationSpot> GetLocationSpots(Location location)
    {
        if (location == null) return new List<LocationSpot>();

        return location.Spots;
    }

    public LocationSpot GetLocationSpotForLocation(string locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        List<LocationSpot> spots = GetLocationSpots(location);
        LocationSpot? locationSpot = spots.FirstOrDefault(x => x.Name == locationSpotName);
        return locationSpot;
    }

    public List<StrategicTag> GetEnvironmentalProperties(string locationName, string locationSpotName)
    {
        Location location = GetLocation(locationName);
        LocationSpot locationSpot = GetLocationSpotForLocation(locationName, locationSpotName);

        return new List<StrategicTag>();
    }
}

/// <summary>
/// Minimal location generator that creates a single location with basic properties.
/// Used at game startup to inject the initial AI-generated location.
/// </summary>
public class InitialLocationGenerator
{
    /// <summary>
    /// Generates a new location based on a minimal prompt.
    /// </summary>
    /// <param name="locationPrompt">Brief description of what to create (e.g., "coastal village")</param>
    /// <returns>A fully constructed Location object with spots and actions</returns>
    public static Location GenerateNewLocation(string locationPrompt)
    {
        // In a real implementation, this would be processed by AI
        // For POC, we'll generate a basic mountain village

        // 1. Create the base location
        Location newLocation = new Location
        {
            Name = "MountainVillage",
            Description = "A small settlement nestled between towering peaks",
            DetailedDescription = "Stone cottages with smoke-billowing chimneys dot the hillside of this secluded mountain village. Towering peaks surround the settlement, offering both protection and isolation. The air is crisp and clean, carrying the scent of pine and woodsmoke.",
            History = "Founded by miners three generations ago, the village has since become a trading post for travelers crossing the mountain pass.",
            PointsOfInterest = "The central square with its ancient well, the blacksmith's forge glowing day and night, the elder's hut perched on the highest point, and the trading post where merchants gather.",

            Difficulty = 1,
            TravelTimeMinutes = 180,
            TravelDescription = "A winding path through rocky terrain leads to this isolated settlement",

            // Connected to existing locations
            ConnectedLocationIds = new List<string> { "Village", "Forest" },

            // Set environmental properties
            EnvironmentalProperties = new List<IEnvironmentalProperty>
            {
                Illumination.Bright,
                Population.Quiet,
            }
        };

        // 2. Create time-based properties
        newLocation.TimeProperties = new Dictionary<string, List<IEnvironmentalProperty>>
        {
            { "Morning", new List<IEnvironmentalProperty>
                {
                    Illumination.Bright,
                    Population.Quiet
                }
            },
            { "Afternoon", new List<IEnvironmentalProperty>
                {
                    Illumination.Bright,
                    Population.Crowded
                }
            },
            { "Evening", new List<IEnvironmentalProperty>
                {
                    Illumination.Shadowy,
                    Population.Crowded
                }
            },
            { "Night", new List<IEnvironmentalProperty>
                {
                    Illumination.Dark,
                    Population.Quiet
                }
            }
        };

        // 3. Create location spots
        newLocation.Spots = new List<LocationSpot>
        {
            // Trading post
            new LocationSpot
            {
                Name = "Trading Post",
                Description = "A sturdy wooden building where travelers and merchants exchange goods",
                LocationName = newLocation.Name,
                InteractionType = "Commercial",
                InteractionDescription = "Buy, sell, or trade goods with traveling merchants",
                Position = "East",
                Accessibility = Population.Crowded,
                RoomLayout = Physical.Confined,
                ActionNames = new List<ActionNames> { ActionNames.TradeGoods }
            },
            
            // Mountain path
            new LocationSpot
            {
                Name = "Mountain Path",
                Description = "A narrow trail leading higher into the mountains",
                LocationName = newLocation.Name,
                InteractionType = "Travel",
                InteractionDescription = "Venture into the dangerous mountain trails",
                Position = "West",
                Accessibility = Population.Isolated,
                RoomLayout = Physical.Hazardous,
                ActionNames = new List<ActionNames> { ActionNames.ForestTravel }
            }
        };

        // Add strategic tags based on the location's environmental properties
        newLocation.StrategicTags = new List<StrategicTag>
        {
            new StrategicTag("Mountain Light", Illumination.Bright),
            new StrategicTag("Secluded Settlement", Population.Quiet),
            new StrategicTag("Survival Economy", Economic.Humble),
            new StrategicTag("Cramped Village", Physical.Confined)
        };

        // Add narrative tags appropriate for this location
        newLocation.NarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.TheoreticalMindset,
            NarrativeTagRepository.DetailFixation
        };

        return newLocation;
    }
}