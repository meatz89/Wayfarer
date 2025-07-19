public class LocationCreationInput
{
    public string CharacterArchetype { get; set; }
    public string LocationName { get; set; }

    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveContracts { get; set; }

    public string ConnectedLocations { get; set; } // All spots across all locations
    public string CurrentLocationSpots { get; set; } // All spots in current location

    public bool WasConversationContext { get; set; }
    public string TravelOrigin { get; set; }
    public string TravelDestination { get; set; }

    public int CurrentDepth { get; set; }
    public int LastHubDepth { get; set; }
}