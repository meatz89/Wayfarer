using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Strongly typed context for narrative generation
/// </summary>
public class NarrativeContext
{
    public string Weather { get; set; }
    public int NpcCount { get; set; }
    public int UrgentLetters { get; set; }
    public TimeBlocks? CurrentTimeBlock { get; set; }
    public string LocationName { get; set; }
    public bool IsIndoor { get; set; }
}

/// <summary>
/// Generates narrative text and atmospheric descriptions for locations and spots.
/// Creates immersive descriptions based on time, properties, and context.
/// </summary>
public class LocationNarrativeGenerator
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;

    public LocationNarrativeGenerator(GameWorld gameWorld, TimeManager timeManager)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    }

    /// <summary>
    /// Generate atmosphere text for a Venue spot.
    /// </summary>
    public string GenerateAtmosphereText(
        LocationSpot spot,
        Venue venue,
        TimeBlocks currentTime,
        int npcsPresent)
    {
        if (spot != null)
        {
            return GenerateSpotAtmosphere(spot, currentTime, npcsPresent);
        }
        else if (venue != null)
        {
            return GenerateLocationAtmosphere(venue, currentTime);
        }

        return "An undefined location.";
    }

    /// <summary>
    /// Generate atmosphere text specific to a spot.
    /// </summary>
    private string GenerateSpotAtmosphere(
        LocationSpot spot,
        TimeBlocks currentTime,
        int npcsPresent)
    {
        SpotDescriptionGenerator descGenerator = new SpotDescriptionGenerator();
        List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);

        // Debug log
        Console.WriteLine($"[LocationNarrativeGenerator] Generating atmosphere for: {spot.Id}");
        Console.WriteLine($"  Properties: {string.Join(", ", activeProperties)}");
        Console.WriteLine($"  Time: {currentTime}, NPCs: {npcsPresent}");

        return descGenerator.GenerateDescription(
            activeProperties,
            currentTime,
            npcsPresent
        );
    }

    /// <summary>
    /// Generate atmosphere text for a general venue.
    /// </summary>
    private string GenerateLocationAtmosphere(Venue venue, TimeBlocks currentTime)
    {
        if (string.IsNullOrEmpty(_gameWorld.Locations.FirstOrDefault(x => x.Id == venue.Id).Description))
        {
            return GenerateDefaultLocationDescription(venue, currentTime);
        }

        // Add time-specific modifiers to the base description
        string timeModifier = GetTimeModifier(currentTime);
        if (!string.IsNullOrEmpty(timeModifier))
        {
            return $"{venue.Description} {timeModifier}";
        }

        return venue.Description;
    }

    /// <summary>
    /// Generate a default description when none is provided.
    /// </summary>
    private string GenerateDefaultLocationDescription(Venue venue, TimeBlocks currentTime)
    {
        string timeDesc = GetTimeDescription(currentTime);

        return $"{timeDesc} at {venue.Name}.";
    }

    /// <summary>
    /// Get a time-specific modifier for descriptions.
    /// </summary>
    private string GetTimeModifier(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "The early morning light casts long shadows.",
            TimeBlocks.Morning => "The morning bustle is beginning to pick up.",
            TimeBlocks.Midday => "The afternoon sun warms the area.",
            TimeBlocks.Afternoon => "Evening approaches, bringing a change of pace.",
            TimeBlocks.Evening => "Night has fallen, bringing quiet to most areas.",
            TimeBlocks.Night => "The deep of night brings stillness.",
            _ => ""
        };
    }

    /// <summary>
    /// Get a general time description.
    /// </summary>
    private string GetTimeDescription(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "It's early dawn",
            TimeBlocks.Morning => "It's morning",
            TimeBlocks.Midday => "It's afternoon",
            TimeBlocks.Afternoon => "It's evening",
            TimeBlocks.Evening => "It's nighttime",
            TimeBlocks.Night => "It's late at night",
            _ => "The time is uncertain"
        };
    }

    /// <summary>
    /// Generate a brief description for entering a new venue.
    /// </summary>
    public string GenerateArrivalText(Venue venue, LocationSpot entrySpot)
    {
        if (venue == null) return "You arrive at an unknown venue.";

        string spotDesc = "";
        if (entrySpot != null)
        {
            spotDesc = $" at {entrySpot.Name}";
        }

        return $"You arrive at {venue.Name}{spotDesc}.";
    }

    /// <summary>
    /// Generate text for leaving a venue.
    /// </summary>
    public string GenerateDepartureText(Venue venue, LocationSpot exitSpot)
    {
        if (venue == null) return "You depart from your current venue.";

        string spotDesc = "";
        if (exitSpot != null)
        {
            spotDesc = $" from {exitSpot.Name}";
        }

        return $"You leave {venue.Name}{spotDesc}.";
    }

    /// <summary>
    /// Generate a description for movement between spots.
    /// </summary>
    public string GenerateMovementText(LocationSpot fromSpot, LocationSpot toSpot)
    {
        if (fromSpot == null || toSpot == null)
            return "You move to a new area.";

        return $"You move from {fromSpot.Name} to {toSpot.Name}.";
    }

    /// <summary>
    /// Generate contextual flavor text based on current conditions.
    /// </summary>
    public string GenerateContextualFlavor(LocationSpot spot, NarrativeContext context)
    {
        List<string> flavorParts = new List<string>();

        // Check for weather effects
        if (!string.IsNullOrEmpty(context?.Weather))
        {
            string weatherFlavor = GetWeatherFlavor(context.Weather, spot);
            if (!string.IsNullOrEmpty(weatherFlavor))
                flavorParts.Add(weatherFlavor);
        }

        // Check for crowding
        if (context?.NpcCount > 3)
        {
            flavorParts.Add("The area is quite crowded.");
        }

        // Check for urgent business
        if (context?.UrgentLetters > 0)
        {
            flavorParts.Add("You feel the focus of urgent obligations.");
        }

        return string.Join(" ", flavorParts);
    }

    /// <summary>
    /// Get weather-specific flavor text.
    /// </summary>
    private string GetWeatherFlavor(string weather, LocationSpot spot)
    {
        // Assume we're outdoors unless spot has warm/shaded properties (indicating shelter)
        bool isIndoor = spot?.SpotProperties?.Contains(SpotPropertyType.Warm) == true ||
                       spot?.SpotProperties?.Contains(SpotPropertyType.Shaded) == true;

        return weather switch
        {
            "Rain" when !isIndoor => "Rain patters steadily around you.",
            "Rain" when isIndoor => "You can hear rain against the windows.",
            "Snow" when !isIndoor => "Snow drifts gently through the air.",
            "Snow" when isIndoor => "Snow accumulates outside the windows.",
            "Wind" when !isIndoor => "A strong wind tugs at your clothes.",
            "Wind" when isIndoor => "Wind rattles the shutters.",
            _ => null
        };
    }

    /// <summary>
    /// Generate observation prompt text.
    /// </summary>
    public string GenerateObservationPrompt(Venue venue, LocationSpot spot)
    {
        if (spot != null)
        {
            List<SpotPropertyType> properties = spot.GetActiveProperties(_timeManager.GetCurrentTimeBlock());
            if (properties.Contains(SpotPropertyType.ViewsMarket))
                return "The area has several interesting details worth observing.";
            if (properties.Contains(SpotPropertyType.Commercial))
                return "The bustling diplomacy provides many things to notice.";
        }

        return "There might be something worth observing here.";
    }
}
