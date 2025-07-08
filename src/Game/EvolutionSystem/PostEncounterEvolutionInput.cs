public class PostEncounterEvolutionInput
{
    public string EncounterNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string EncounterOutcome { get; set; }

    public string KnownLocations { get; set; }
    public string ConnectedLocations { get; set; }
    public string CurrentLocationSpots { get; set; }

    public string KnownCharacters { get; set; }
    public string ActiveContracts { get; set; }
    public string AllExistingActions { get; set; }

    public int CurrentDepth { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
}
