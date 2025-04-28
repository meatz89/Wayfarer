public class SerializablePlayerState
{
    // Core identity - null if character creation needed
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Archetype { get; set; }

    // Resources
    public int Coins { get; set; }
    public int Food { get; set; }
    public int MedicinalHerbs { get; set; }
    public int Health { get; set; }
    public int Energy { get; set; }
    public int Concentration { get; set; }
    public int Confidence { get; set; }

    // Progression
    public int Level { get; set; }
    public int CurrentXP { get; set; }

    // Simple inventory
    public List<string> InventoryItems { get; set; } = new List<string>();

    // Skills
    public List<SerializableSkill> Skills { get; set; } = new List<SerializableSkill>();
}
