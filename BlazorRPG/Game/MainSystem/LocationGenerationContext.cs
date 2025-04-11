public class LocationCreationInput
{
    public string CharacterArchetype { get; internal set; }
    public string LocationName { get; internal set; }

    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveOpportunities { get; set; }

    public string ConnectedLocations { get; set; } // All spots across all locations
    public string CurrentLocationSpots { get; set; } // All spots in current location
    public string AllExistingActions { get; set; } // All existing actions

    public bool WasTravelEncounter { get; set; }
    public string TravelOrigin { get; set; }
    public string TravelDestination { get; set; }

    public int CurrentDepth { get; set; }
    public int LastHubDepth { get; set; }
}