using BlazorRPG.Game.EncounterManager;
using static LocationInfo;

/// <summary>
/// Represents a location in the game where encounters take place
/// </summary>
public class LocationInfo
{
    public string Name { get; }
    public List<ApproachTypes> FavoredApproaches { get; } = new List<ApproachTypes>();
    public List<ApproachTypes> DisfavoredApproaches { get; } = new List<ApproachTypes>();
    public List<FocusTags> FavoredFocuses { get; } = new List<FocusTags>();
    public List<FocusTags> DisfavoredFocuses { get; } = new List<FocusTags>();
    public List<IEncounterTag> AvailableTags { get; } = new List<IEncounterTag>();

    // Success thresholds
    public int PartialThreshold { get; }
    public int StandardThreshold { get; }
    public int ExceptionalThreshold { get; }

    // Encounter duration in turns
    public int TurnDuration { get; }

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
    public ItemTypes ResourceType { get; }
    public bool PlayerKnowledge { get; }

    public LocationInfo(
        string name,
        List<ApproachTypes> favoredApproaches,
        List<ApproachTypes> disfavoredApproaches,
        List<FocusTags> favoredFocuses,
        List<FocusTags> disfavoredFocuses,
        int duration,
        int partialThreshold,
        int standardThreshold,
        int exceptionalThreshold,
        HostilityLevels hostility,
        PresentationStyles style)
    {
        Name = name;
        FavoredApproaches = favoredApproaches;
        DisfavoredApproaches = disfavoredApproaches;
        FavoredFocuses = favoredFocuses;
        DisfavoredFocuses = disfavoredFocuses;

        PartialThreshold = partialThreshold;
        StandardThreshold = standardThreshold;
        ExceptionalThreshold = exceptionalThreshold;

        TurnDuration = duration;
        Hostility = hostility;
        Style = style;
    }

    public void AddTag(NarrativeTag narrativeTag)
    {
        AvailableTags.Add(narrativeTag);

    }
    public void AddTag(StrategicTag strategicTag)
    {
        AvailableTags.Add(strategicTag);
    }

    // In the LocationInfo class, add a method to get environmental pressure
    public int GetEnvironmentalPressure(int turnNumber)
    {
        switch (Hostility)
        {
            case HostilityLevels.Hostile:
                return 1; // +1 pressure per turn in hostile locations
            case HostilityLevels.Neutral:
                return turnNumber > 3 ? 1 : 0; // +1 pressure after turn 3 in neutral locations
            default:
                return 0; // No environmental pressure in friendly locations
        }
    }
}
