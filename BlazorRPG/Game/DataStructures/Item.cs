public class Item
{
    public ItemNames Name { get; set; }
    public ItemTypes ItemType { get; set; }
    public string Description { get; set; }
    public List<ActionModifier> ActionModifiers { get; set; }
}
