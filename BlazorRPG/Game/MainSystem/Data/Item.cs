public class Item
{
    public string Name { get; set; }
    public ItemTypes ItemType { get; set; }
    public int Condition { get; set; } 

    public Item(string name, ItemTypes itemType)
    {
        Name = name;
        ItemType = itemType;
        Condition = 100;
    }
}