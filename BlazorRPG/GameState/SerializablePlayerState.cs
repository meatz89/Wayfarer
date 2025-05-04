public class SerializablePlayerState
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Archetype { get; set; }

    // Resources
    public int Coins { get; set; }

    // Progression
    public int Level { get; set; }
    public int CurrentXP { get; set; }

    // Simple inventory
    public List<string> InventoryItems { get; set; } = new List<string>();

    // Skills
    public List<SerializableSkill> Skills { get; set; } = new List<SerializableSkill>();
}
