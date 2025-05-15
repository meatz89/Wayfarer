public class SerializablePlayerState
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Archetype { get; set; }
    public int Coins { get; set; }
    public int MaxActionPoints { get; set; }
    public int ActionPoints { get; set; }
    public int MaxEnergy { get; set; }
    public int Energy { get; set; }
    public int MaxHealth { get; set; }
    public int Health { get; set; }
    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public List<string> InventoryItems { get; set; } = new List<string>();
    public List<string> SelectedCards { get; set; } = new List<string>();
}