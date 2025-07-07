public class WorldStateInput
{
    public string PlayerArchetype { get; set; }

    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Concentration { get; set; }
    public int MaxConcentration { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public int Coins { get; set; }

    public string CurrentLocation { get; set; }
    public int LocationDepth { get; set; }
    public string CurrentSpot { get; set; }
    public string ConnectedLocations { get; set; }
    public string LocationSpots { get; set; }

    public string Inventory { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveContracts { get; set; }

    public string MemorySummary { get; set; }
    public List<NPC> Characters { get; set; }
    public RelationshipList RelationshipList { get; set; }
}
