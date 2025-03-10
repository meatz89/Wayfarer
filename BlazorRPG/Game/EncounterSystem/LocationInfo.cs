using BlazorRPG.Game.EncounterManager;

/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class LocationInfo
{
    public string Name { get; }
    public List<ApproachTypes> FavoredApproaches { get; }
    public List<ApproachTypes> DisfavoredApproaches { get; }
    public List<FocusTags> FavoredFocuses { get; }
    public List<FocusTags> DisfavoredFocuses { get; }
    public List<IEncounterTag> AvailableTags { get; }

    // Success thresholds
    public int PartialThreshold { get; }
    public int StandardThreshold { get; }
    public int ExceptionalThreshold { get; }

    // Encounter duration in turns
    public int Duration { get; }

    // How hostile is this location (affects tag balance)
    public enum HostilityLevels { Friendly, Neutral, Hostile }
    public HostilityLevels Hostility { get; }

    // Presentation style for this location
    public PresentationStyles Style { get; }

    public LocationTypes LocationType { get; set; } // Industrial/Commercial/etc
    public LocationNames LocationName { get; set; }
    public List<LocationNames> TravelConnections { get; set; }
    public List<LocationSpot> LocationSpots { get; set; } // Action groupings
    public int Difficulty { get; set; }
    public LocationArchetypes LocationArchetype { get; }
    public CrowdDensity CrowdDensity { get; }
    public OpportunityTypes Opportunity { get; }
    public ResourceTypes ResourceType { get; }
    public bool PlayerKnowledge { get; }

    public LocationInfo(
        string name,
        List<ApproachTypes> favoredApproaches,
        List<ApproachTypes> disfavoredApproaches,
        List<FocusTags> favoredFocuses,
        List<FocusTags> disfavoredFocuses,
        List<IEncounterTag> availableTags,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        int duration,
        HostilityLevels hostility,
        PresentationStyles style)
    {
        Name = name;
        FavoredApproaches = favoredApproaches;
        DisfavoredApproaches = disfavoredApproaches;
        FavoredFocuses = favoredFocuses;
        DisfavoredFocuses = disfavoredFocuses;
        AvailableTags = availableTags;

        PartialThreshold = partialThreshold;
        StandardThreshold = standardThreshold;
        ExceptionalThreshold = exceptionalThreshold;

        Duration = duration;
        Hostility = hostility;
        Style = style;
    }
}
