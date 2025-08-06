using System;
using System.Collections.Generic;

/// <summary>
/// Factory for creating items with guaranteed valid data.
/// Items don't reference other entities, so they can be created directly.
/// </summary>
public class ItemFactory
{
    public ItemFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a minimal item with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public Item CreateMinimalItem(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Item ID cannot be empty", nameof(id));

        string name = FormatIdAsName(id);

        return new Item
        {
            Id = id,
            Name = name,
            Weight = 1,
            BuyPrice = 5, // Basic price
            SellPrice = 2, // Lower sell price
            InventorySlots = 1,
            Size = SizeCategory.Small,
            Categories = new List<ItemCategory>(),
            Description = $"An item called {name}"
        };
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create an item with validated data.
    /// Items are standalone entities without references to other game objects.
    /// </summary>
    public Item CreateItem(
        string id,
        string name,
        int weight = 1,
        int buyPrice = 0,
        int sellPrice = 0,
        int inventorySlots = 1,
        SizeCategory sizeCategory = SizeCategory.Small,
        List<ItemCategory> categories = null,
        string description = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Item ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Item name cannot be empty", nameof(name));
        if (inventorySlots < 1)
            throw new ArgumentException("Items must take at least 1 inventory slot", nameof(inventorySlots));

        Item item = new Item
        {
            Id = id,
            Name = name,
            Weight = weight,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            InventorySlots = inventorySlots,
            Size = sizeCategory,
            Categories = categories ?? new List<ItemCategory>(),
            Description = description ?? $"A {sizeCategory.ToString().ToLower()} {name.ToLower()}"
        };

        // Validate price logic
        if (sellPrice > buyPrice)
        {
            Console.WriteLine($"WARNING: Item '{id}' has sell price ({sellPrice}) higher than buy price ({buyPrice}). This may create infinite money exploits.");
        }

        // Validate size vs slots
        ValidateSizeVsSlots(item);

        return item;
    }

    /// <summary>
    /// Validate that item size category matches inventory slots
    /// </summary>
    private void ValidateSizeVsSlots(Item item)
    {
        int expectedSlots = item.Size switch
        {
            SizeCategory.Tiny => 1,
            SizeCategory.Small => 1,
            SizeCategory.Medium => 1,
            SizeCategory.Large => 2,
            SizeCategory.Massive => 3,
            _ => 1
        };

        if (item.InventorySlots != expectedSlots)
        {
            Console.WriteLine($"WARNING: Item '{item.Id}' size category {item.Size} suggests {expectedSlots} slots but has {item.InventorySlots} slots configured.");
        }
    }

    /// <summary>
    /// Create an item suitable for trading based on category
    /// </summary>
    public Item CreateTradeGood(
        string id,
        string name,
        ItemCategory category,
        int basePrice,
        SizeCategory size = SizeCategory.Small,
        string description = null)
    {
        // Calculate buy/sell prices with reasonable margins
        int buyPrice = basePrice;
        int sellPrice = (int)(basePrice * 0.7); // 30% markdown for selling

        // Adjust slots based on category
        int slots = category switch
        {
            ItemCategory.Bulk_Goods => 2,
            ItemCategory.Luxury_Items => 1,
            _ => 1
        };

        return CreateItem(
            id: id,
            name: name,
            buyPrice: buyPrice,
            sellPrice: sellPrice,
            inventorySlots: slots,
            sizeCategory: size,
            categories: new List<ItemCategory> { category },
            description: description
        );
    }
}