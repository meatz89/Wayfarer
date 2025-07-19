public class PostConversationEvolutionInput
{
    public string ConversationNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string ConversationOutcome { get; set; }

    public string KnownLocations { get; set; }
    public string ConnectedLocations { get; set; }
    public string CurrentLocationSpots { get; set; }

    public string KnownCharacters { get; set; }
    public string ActiveContracts { get; set; }

    public int CurrentDepth { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
}
