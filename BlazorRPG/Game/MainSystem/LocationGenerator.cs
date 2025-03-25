
/// <summary>
/// Minimal location generator that creates a single location with basic properties.
/// Uses the NarrativeService to fill in details based on a general location type.
/// </summary>
public class LocationGenerator
{
    public async Task<Location> GenerateNewLocationAsync(string locationType, NarrativeService narrativeService)
    {
        // Create context for location generation
        LocationGenerationContext context = new LocationGenerationContext
        {
            LocationType = locationType,
            RequestedSpotCount = 2,
            Difficulty = DetermineDifficulty(locationType)
        };

        // Get location details from AI
        LocationDetails details = await narrativeService.GenerateLocationDetailsAsync(context);

        // Convert SpotDetails to LocationSpot objects
        List<LocationSpot> locationSpots = new List<LocationSpot>();
        foreach (SpotDetails spotDetail in details.Spots)
        {
            LocationSpot spot = new LocationSpot
            {
                Name = spotDetail.Name,
                Description = spotDetail.Description,
                LocationName = details.Name,
                InteractionType = spotDetail.InteractionType,
                InteractionDescription = spotDetail.InteractionDescription,
                Position = spotDetail.Position,
                ActionNames = new List<ActionNames>(spotDetail.ActionNames)
            };

            locationSpots.Add(spot);
        }

        // Create the location with converted spots
        Location location = new Location
        {
            Name = details.Name,
            Description = details.Description,
            DetailedDescription = details.DetailedDescription,
            History = details.History,
            PointsOfInterest = details.PointsOfInterest,
            Difficulty = context.Difficulty,
            TravelTimeMinutes = details.TravelTimeMinutes,
            TravelDescription = details.TravelDescription,
            ConnectedLocationIds = details.ConnectedLocationIds,
            EnvironmentalProperties = details.EnvironmentalProperties,
            TimeProperties = details.TimeProperties,
            Spots = locationSpots,
            StrategicTags = details.StrategicTags,
            NarrativeTags = details.NarrativeTags
        };

        return location;
    }

    private static int DetermineDifficulty(string locationType)
    {
        // Determine base difficulty by location type
        switch (locationType.ToLower())
        {
            case "village":
            case "tavern":
            case "town":
                return 1;

            case "forest":
            case "cave":
            case "ruins":
                return 2;

            case "mountain":
            case "dungeon":
            case "wilderness":
                return 3;

            default:
                return 1;
        }
    }
}
