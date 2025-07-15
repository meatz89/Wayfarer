public class SerializablePlayerState
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Archetype { get; set; }
    public int Coins { get; set; }
    public int MaxStamina { get; set; }
    public int Stamina { get; set; }
    public int MaxHealth { get; set; }
    public int Health { get; set; }
    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public List<string> InventoryItems { get; set; } = new List<string>();
    public List<string> SelectedCards { get; set; } = new List<string>();
}