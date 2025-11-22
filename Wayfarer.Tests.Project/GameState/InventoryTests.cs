using Xunit;

namespace Wayfarer.Tests.GameState;

/// <summary>
/// Comprehensive tests for Inventory capacity enforcement system.
/// Tests critical weight-based inventory management: capacity limits,
/// transport bonuses, boundary conditions, weight calculations, and item management.
/// MANDATORY per CLAUDE.md: Business logic requires complete test coverage.
/// </summary>
public class InventoryTests
{
    private const int BASE_CAPACITY = 10;
    private const int CART_BONUS = 6;
    private const int CARRIAGE_BONUS = 3;

    [Fact]
    public void AddItemWithWeightCheck_WithinCapacity_Succeeds()
    {
        // Arrange: Inventory with base capacity 10, item weight 3
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("test_item", weight: 3);

        // Act
        bool added = inventory.AddItemWithWeightCheck(item, TravelMethods.Walking);

        // Assert
        Assert.True(added);
        Assert.Equal(3, inventory.GetUsedWeight());
        Assert.Contains(item, inventory.GetAllItems());
    }

    [Fact]
    public void AddItemWithWeightCheck_ExceedsCapacity_Fails()
    {
        // Arrange: Inventory with 8 weight used, trying to add 4 weight item (total 12 > 10)
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item existingItem = CreateTestItem("existing", weight: 8);
        inventory.Add(existingItem);

        Item newItem = CreateTestItem("new_item", weight: 4);

        // Act
        bool added = inventory.AddItemWithWeightCheck(newItem, TravelMethods.Walking);

        // Assert: Item not added (exceeds capacity)
        Assert.False(added);
        Assert.Equal(8, inventory.GetUsedWeight());
        Assert.DoesNotContain(newItem, inventory.GetAllItems());
    }

    [Fact]
    public void AddItemWithWeightCheck_ExactlyAtCapacity_Succeeds()
    {
        // Arrange: Inventory with 6 weight used, adding 4 weight item (total = 10)
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item existingItem = CreateTestItem("existing", weight: 6);
        inventory.Add(existingItem);

        Item newItem = CreateTestItem("new_item", weight: 4);

        // Act: Exactly at capacity boundary
        bool added = inventory.AddItemWithWeightCheck(newItem, TravelMethods.Walking);

        // Assert: Item added (at capacity is allowed)
        Assert.True(added);
        Assert.Equal(10, inventory.GetUsedWeight());
        Assert.Contains(newItem, inventory.GetAllItems());
    }

    [Fact]
    public void AddItemWithWeightCheck_OneOverCapacity_Fails()
    {
        // Arrange: Inventory with 10 weight used (at capacity), adding 1 weight item
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item existingItem = CreateTestItem("existing", weight: 10);
        inventory.Add(existingItem);

        Item newItem = CreateTestItem("new_item", weight: 1);

        // Act: One over capacity
        bool added = inventory.AddItemWithWeightCheck(newItem, TravelMethods.Walking);

        // Assert: Item not added
        Assert.False(added);
        Assert.Equal(10, inventory.GetUsedWeight());
        Assert.DoesNotContain(newItem, inventory.GetAllItems());
    }

    [Fact]
    public void AddItemWithWeightCheck_WithCart_AppliesBonus()
    {
        // Arrange: Cart adds +6 capacity (base 10 + 6 = 16 total)
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("heavy_item", weight: 14);

        // Act: Without Cart would fail (14 > 10), with Cart succeeds (14 <= 16)
        bool added = inventory.AddItemWithWeightCheck(item, TravelMethods.Cart);

        // Assert: Item added due to Cart bonus
        Assert.True(added);
        Assert.Equal(14, inventory.GetUsedWeight());
        Assert.Contains(item, inventory.GetAllItems());
    }

    [Fact]
    public void AddItemWithWeightCheck_WithCarriage_AppliesBonus()
    {
        // Arrange: Carriage adds +3 capacity (base 10 + 3 = 13 total)
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("item", weight: 12);

        // Act: Without Carriage would fail (12 > 10), with Carriage succeeds (12 <= 13)
        bool added = inventory.AddItemWithWeightCheck(item, TravelMethods.Carriage);

        // Assert: Item added due to Carriage bonus
        Assert.True(added);
        Assert.Equal(12, inventory.GetUsedWeight());
    }

    [Fact]
    public void AddItemWithWeightCheck_ExceedsEvenWithCartBonus_Fails()
    {
        // Arrange: Cart capacity = 16, item weight = 17
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("massive_item", weight: 17);

        // Act: Even with Cart bonus, item too heavy
        bool added = inventory.AddItemWithWeightCheck(item, TravelMethods.Cart);

        // Assert: Item not added
        Assert.False(added);
        Assert.Equal(0, inventory.GetUsedWeight());
    }

    [Fact]
    public void AddItemWithWeightCheck_NullItem_ThrowsArgumentNullException()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => inventory.AddItemWithWeightCheck(null, TravelMethods.Walking)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public void CanAddItem_WithinCapacity_ReturnsTrue()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item existingItem = CreateTestItem("existing", weight: 5);
        inventory.Add(existingItem);

        Item newItem = CreateTestItem("new", weight: 4);

        // Act: 5 + 4 = 9 <= 10
        bool canAdd = inventory.CanAddItem(newItem);

        // Assert
        Assert.True(canAdd);
    }

    [Fact]
    public void CanAddItem_ExceedsCapacity_ReturnsFalse()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item existingItem = CreateTestItem("existing", weight: 8);
        inventory.Add(existingItem);

        Item newItem = CreateTestItem("new", weight: 3);

        // Act: 8 + 3 = 11 > 10
        bool canAdd = inventory.CanAddItem(newItem);

        // Assert
        Assert.False(canAdd);
    }

    [Fact]
    public void CanAddItem_NullItem_ReturnsFalse()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        bool canAdd = inventory.CanAddItem(null);

        // Assert: Null item returns false (safe handling)
        Assert.False(canAdd);
    }

    [Fact]
    public void GetMaxWeight_WithWalking_ReturnsBaseCapacity()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int maxWeight = inventory.GetMaxWeight(TravelMethods.Walking);

        // Assert: No bonus for walking
        Assert.Equal(BASE_CAPACITY, maxWeight);
    }

    [Fact]
    public void GetMaxWeight_WithCart_ReturnsBaseCapacityPlusBonus()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int maxWeight = inventory.GetMaxWeight(TravelMethods.Cart);

        // Assert: Base + Cart bonus (10 + 6 = 16)
        Assert.Equal(BASE_CAPACITY + CART_BONUS, maxWeight);
    }

    [Fact]
    public void GetMaxWeight_WithCarriage_ReturnsBaseCapacityPlusBonus()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int maxWeight = inventory.GetMaxWeight(TravelMethods.Carriage);

        // Assert: Base + Carriage bonus (10 + 3 = 13)
        Assert.Equal(BASE_CAPACITY + CARRIAGE_BONUS, maxWeight);
    }

    [Fact]
    public void GetMaxWeight_WithHorseback_ReturnsBaseCapacity()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int maxWeight = inventory.GetMaxWeight(TravelMethods.Horseback);

        // Assert: No bonus for horseback
        Assert.Equal(BASE_CAPACITY, maxWeight);
    }

    [Fact]
    public void GetMaxWeight_WithBoat_ReturnsBaseCapacity()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int maxWeight = inventory.GetMaxWeight(TravelMethods.Boat);

        // Assert: No bonus for boat
        Assert.Equal(BASE_CAPACITY, maxWeight);
    }

    [Fact]
    public void GetMaxWeight_NullTransport_ReturnsBaseCapacity()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act: No transport method provided
        int maxWeight = inventory.GetMaxWeight(null);

        // Assert: Returns base capacity
        Assert.Equal(BASE_CAPACITY, maxWeight);
    }

    [Fact]
    public void GetUsedWeight_EmptyInventory_ReturnsZero()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int usedWeight = inventory.GetUsedWeight();

        // Assert
        Assert.Equal(0, usedWeight);
    }

    [Fact]
    public void GetUsedWeight_WithItems_ReturnsSumOfWeights()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item1 = CreateTestItem("item1", weight: 3);
        Item item2 = CreateTestItem("item2", weight: 5);
        Item item3 = CreateTestItem("item3", weight: 2);

        inventory.Add(item1);
        inventory.Add(item2);
        inventory.Add(item3);

        // Act
        int usedWeight = inventory.GetUsedWeight();

        // Assert: 3 + 5 + 2 = 10
        Assert.Equal(10, usedWeight);
    }

    [Fact]
    public void RemoveItems_FreesCapacity()
    {
        // Arrange: Fill inventory to capacity
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item1 = CreateTestItem("item1", weight: 6);
        Item item2 = CreateTestItem("item2", weight: 4);
        inventory.Add(item1);
        inventory.Add(item2);

        Assert.Equal(10, inventory.GetUsedWeight()); // At capacity

        // Act: Remove one item
        int removed = inventory.RemoveItems(item1, 1);

        // Assert: Capacity freed
        Assert.Equal(1, removed);
        Assert.Equal(4, inventory.GetUsedWeight());

        // Can now add new item
        Item newItem = CreateTestItem("new", weight: 5);
        bool canAdd = inventory.CanAddItem(newItem);
        Assert.True(canAdd);
    }

    [Fact]
    public void AddItemWithWeightCheck_AfterRemoval_Succeeds()
    {
        // Arrange: Fill inventory
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item1 = CreateTestItem("item1", weight: 8);
        inventory.Add(item1);

        Item newItem = CreateTestItem("new", weight: 5);
        Assert.False(inventory.AddItemWithWeightCheck(newItem, TravelMethods.Walking)); // Fails initially

        // Act: Remove item, then add
        inventory.Remove(item1);
        bool added = inventory.AddItemWithWeightCheck(newItem, TravelMethods.Walking);

        // Assert: Now succeeds
        Assert.True(added);
        Assert.Equal(5, inventory.GetUsedWeight());
    }

    [Fact]
    public void HasFreeWeight_WithSpace_ReturnsTrue()
    {
        // Arrange: Inventory with space
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("item", weight: 5);
        inventory.Add(item);

        // Act
        bool hasFree = inventory.HasFreeWeight();

        // Assert: 5 < 10, has free space
        Assert.True(hasFree);
    }

    [Fact]
    public void HasFreeWeight_AtCapacity_ReturnsFalse()
    {
        // Arrange: Inventory at capacity
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("item", weight: 10);
        inventory.Add(item);

        // Act
        bool hasFree = inventory.HasFreeWeight();

        // Assert: 10 >= 10, no free space
        Assert.False(hasFree);
    }

    [Fact]
    public void IsFull_AtCapacity_ReturnsTrue()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("item", weight: 10);
        inventory.Add(item);

        // Act
        bool isFull = inventory.IsFull();

        // Assert
        Assert.True(isFull);
    }

    [Fact]
    public void IsFull_BelowCapacity_ReturnsFalse()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        Item item = CreateTestItem("item", weight: 5);
        inventory.Add(item);

        // Act
        bool isFull = inventory.IsFull();

        // Assert
        Assert.False(isFull);
    }

    [Fact]
    public void AddItemWithWeightCheck_MultipleItemsGradualFill_EnforcesCapacity()
    {
        // Arrange: Gradually fill inventory
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act: Add items until capacity reached
        Item item1 = CreateTestItem("item1", weight: 3);
        Item item2 = CreateTestItem("item2", weight: 3);
        Item item3 = CreateTestItem("item3", weight: 3);
        Item item4 = CreateTestItem("item4", weight: 2); // Total would be 11

        Assert.True(inventory.AddItemWithWeightCheck(item1, TravelMethods.Walking)); // 3
        Assert.True(inventory.AddItemWithWeightCheck(item2, TravelMethods.Walking)); // 6
        Assert.True(inventory.AddItemWithWeightCheck(item3, TravelMethods.Walking)); // 9
        Assert.False(inventory.AddItemWithWeightCheck(item4, TravelMethods.Walking)); // 11 > 10, fails

        // Assert: Capacity enforced
        Assert.Equal(9, inventory.GetUsedWeight());
        Assert.Equal(3, inventory.GetAllItems().Count);
    }

    [Fact]
    public void GetCapacity_ReturnsMaxWeight()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);

        // Act
        int capacity = inventory.GetCapacity();

        // Assert
        Assert.Equal(BASE_CAPACITY, capacity);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        Inventory inventory = new Inventory(BASE_CAPACITY);
        inventory.Add(CreateTestItem("item1", weight: 3));
        inventory.Add(CreateTestItem("item2", weight: 5));

        // Act
        inventory.Clear();

        // Assert
        Assert.Equal(0, inventory.Count(CreateTestItem("item1", weight: 3)));
        Assert.Equal(0, inventory.GetUsedWeight());
    }

    [Fact]
    public void AddItemWithWeightCheck_CustomCapacity_RespectsCustomValue()
    {
        // Arrange: Custom capacity of 20
        int customCapacity = 20;
        Inventory inventory = new Inventory(customCapacity);
        Item item = CreateTestItem("item", weight: 15);

        // Act
        bool added = inventory.AddItemWithWeightCheck(item, TravelMethods.Walking);

        // Assert: 15 <= 20, succeeds
        Assert.True(added);
        Assert.Equal(15, inventory.GetUsedWeight());
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test item with specified weight
    /// </summary>
    private Item CreateTestItem(string name, int weight)
    {
        return new Item
        {
            Name = name,
            Weight = weight,
            Description = $"Test item {name}"
        };
    }
}
