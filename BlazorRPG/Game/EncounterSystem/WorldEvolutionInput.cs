public class PostEncounterEvolutionInput
{
    public string EncounterNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveOpportunities { get; set; }
    public string EncounterOutcome { get; set; }

    public string CurrentLocationSpots { get; set; } // All spots in current location
    public string AllKnownLocationSpots { get; set; } // All spots across all locations
    public string AllExistingActions { get; set; } // All existing actions

    public bool WasTravelEncounter { get; set; }
    public string TravelDestination { get; set; }

    public int CurrentDepth { get; set; }
    public int LastHubDepth { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
}
