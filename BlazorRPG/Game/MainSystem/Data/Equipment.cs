public class Equipment
{
    public Item MainHand { get; set; }
    public Item Clothing { get; set; }

    public Equipment()
    {
        MainHand = null;
        Clothing = null;
    }

    public List<Item> GetEquippedItems()
    {
        List<Item> items = new List<Item>();

        if (MainHand != null)
            items.Add(MainHand);

        return items;
    }

    public List<Item> GetWornClothing()
    {
        List<Item> items = new List<Item>();

        return items;
    }

    public void SetMainHand(Item item)
    {
        MainHand = item;
    }

    public void SetClothes(Item item)
    {
        Clothing = item;
    }
}