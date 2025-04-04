public class WorldEvolutionInput
{
    public string EncounterNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveOpportunities { get; set; }
    public string EncounterOutcome { get; set; }

    public int CurrentDepth { get; set; }
    public int LastHubDepth { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Energy { get; set; }
    public int MaxEnergy { get; set; }
}