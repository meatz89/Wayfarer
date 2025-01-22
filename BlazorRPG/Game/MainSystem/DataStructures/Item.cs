
public class Item
{
    public string Name { get; set; }
    public ItemTypes ItemType { get; set; }
    public int Condition { get; set; } // Could be a percentage or a discrete value
    public List<ActionModifier> ActionModifiers = new();

    // Add other properties as needed (e.g., effects, value)
    public Item(string name, ItemTypes itemType)
    {
        Name = name;
        ItemType = itemType;
        Condition = 100;
    }
}