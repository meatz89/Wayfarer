public class WorldLocationsContent
{
    public static string GetStartingLocation()
    {
        return CreateGreenvaleVillage().Name;
    }

    public static List<Location> AllLocations = new List<Location>()
    {
        CreateGreenvaleVillage(),
    };

    public static List<LocationSpot> AllLocationSpots
    {
        get
        {
            return CreateVillageSpots();
        }
    }

    public static Location CreateGreenvaleVillage()
    {
        return new Location("Village")
        {
            Description = "A peaceful village surrounded by lush farmland and shaded by ancient oaks.",
            DetailedDescription = "Smoke curls from thatched cottage chimneys, villagers tend small gardens, and the distant church bells echo through the green fields.",
            ConnectedTo = new List<string> { "Forest Trail", "Stone Bridge" }
        };
    }

    public static List<LocationSpot> CreateVillageSpots()
    {
        return new List<LocationSpot>
        {
            new LocationSpot("Maren the Innkeeper", "Village")
            {
                Description = "A warm-faced woman wiping down the bar and greeting weary travelers.",
                CurrentLevel = 1,
                CurrentSpotXP = 0,
                SpotLevels = new List<SpotLevel>()
                {
                    new SpotLevel
                    {
                        Level = 1,
                        AddedActionIds = new List<string>
                        {
                            "purchase_simple_room",
                            "share_evening_story"
                        }
                    },
                    new SpotLevel
                    {
                        Level = 2,
                        EncounterActionId = "host_festival_preparation",
                        RemovedActionIds = new List<string> { "share_evening_story" },
                        AddedActionIds = new List<string> { "negotiate_tavern_trade" }
                    }
                }
            },
        };
    }
}