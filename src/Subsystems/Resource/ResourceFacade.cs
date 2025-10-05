using System;

/// <summary>
/// Public facade for all resource-related operations.
/// Manages coins, health, hunger, and attention across the game.
/// </summary>
public class ResourceFacade
{
    private readonly GameWorld _gameWorld;
    private readonly CoinManager _coinManager;
    private readonly HealthManager _healthManager;
    private readonly HungerManager _hungerManager;
    private readonly ResourceCalculator _resourceCalculator;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly ItemRepository _itemRepository;

    public ResourceFacade(
        GameWorld gameWorld,
        CoinManager coinManager,
        HealthManager healthManager,
        HungerManager hungerManager,
        ResourceCalculator resourceCalculator,
        MessageSystem messageSystem,
        TimeManager timeManager,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _coinManager = coinManager;
        _healthManager = healthManager;
        _hungerManager = hungerManager;
        _resourceCalculator = resourceCalculator;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _itemRepository = itemRepository;
    }

    // ========== COIN OPERATIONS ==========

    public int GetCoins()
    {
        return _coinManager.GetCurrentCoins(_gameWorld.GetPlayer());
    }

    public bool CanAfford(int amount)
    {
        return _coinManager.CanAfford(_gameWorld.GetPlayer(), amount);
    }

    public bool SpendCoins(int amount, string reason)
    {
        return _coinManager.SpendCoins(_gameWorld.GetPlayer(), amount, reason, _messageSystem);
    }

    public bool SpendCoins(int amount)
    {
        return SpendCoins(amount, "Travel cost");
    }

    public void AddCoins(int amount, string source)
    {
        _coinManager.AddCoins(_gameWorld.GetPlayer(), amount, source, _messageSystem);
    }

    // ========== HEALTH OPERATIONS ==========

    public int GetHealth()
    {
        return _healthManager.GetCurrentHealth(_gameWorld.GetPlayer());
    }

    public void TakeDamage(int amount, string source)
    {
        _healthManager.TakeDamage(_gameWorld.GetPlayer(), amount, source, _messageSystem);
    }

    public void Heal(int amount, string source)
    {
        _healthManager.Heal(_gameWorld.GetPlayer(), amount, source, _messageSystem);
    }

    public bool IsAlive()
    {
        return _healthManager.IsAlive(_gameWorld.GetPlayer());
    }

    // ========== HUNGER OPERATIONS ==========

    public int GetHunger()
    {
        return _hungerManager.GetCurrentHunger(_gameWorld.GetPlayer());
    }

    public void IncreaseHunger(int amount, string reason)
    {
        _hungerManager.IncreaseHunger(_gameWorld.GetPlayer(), amount, reason, _messageSystem);
    }

    public void DecreaseHunger(int amount, string source)
    {
        _hungerManager.DecreaseHunger(_gameWorld.GetPlayer(), amount, source, _messageSystem);
    }

    public bool IsStarving()
    {
        return _hungerManager.IsStarving(_gameWorld.GetPlayer());
    }


    // ========== RESOURCE CALCULATIONS ==========

    public int CalculateCarryCapacity()
    {
        int health = GetHealth();
        return _resourceCalculator.CalculateFocusLimit(health);
    }


    public void ProcessTimeBlockTransition(TimeBlocks oldBlock, TimeBlocks newBlock)
    {
        // Increase hunger by 20 per time period
        IncreaseHunger(20, "Time passes");


        // Apply daily NPC decay when transitioning to Dawn (morning refresh)
        if (newBlock == TimeBlocks.Dawn)
        {
            ApplyNPCDailyDecay();
        }
    }

    private void ApplyNPCDailyDecay()
    {
        // Apply daily decay for all NPCs at dawn
        foreach (NPC npc in _gameWorld.GetAllNPCs())
        {
            // Apply daily decay to flow and connection state
            // This moves relationships toward neutral over time
            npc.ApplyDailyDecay();
        }

        _messageSystem.AddSystemMessage(
            "🌅 A new day dawns. Relationships naturally shift toward neutral over time.",
            SystemMessageTypes.Info);
    }

    // ========== COMBINED RESOURCE OPERATIONS ==========

    /// <summary>
    /// Get complete player resources for UI display
    /// </summary>
    public PlayerResourcesInfo GetPlayerResources()
    {
        return new PlayerResourcesInfo(
            coins: GetCoins(),
            health: GetHealth(),
            hunger: GetHunger()
        );
    }

    // ========== INVENTORY OPERATIONS (delegated from Player) ==========

    public Inventory GetInventory()
    {
        return _gameWorld.GetPlayer().Inventory;
    }

    public InventoryViewModel GetInventoryViewModel()
    {
        Inventory inventory = GetInventory();
        return new InventoryViewModel
        {
            Items = inventory.GetItemIds().Select(itemId =>
            {
                Item? item = _itemRepository.GetItemById(itemId);
                return new InventoryItemViewModel
                {
                    ItemId = itemId,
                    Name = item?.Name ?? itemId,
                    Description = item?.Description ?? "",
                    Weight = item?.InitiativeCost ?? 1,
                    Value = item?.SellPrice ?? 0,
                    CanRead = item?.Categories.Contains(ItemCategory.Special_Document) ?? false
                };
            }).ToList(),
            TotalWeight = CalculateTotalWeight(),
            MaxSlots = inventory.GetCapacity(),
            UsedSlots = inventory.GetAllItems().Count,
            Coins = GetCoins()
        };
    }

    public int CalculateTotalWeight()
    {
        Inventory inventory = GetInventory();
        int totalWeight = 0;

        foreach (string itemId in inventory.GetItemIds())
        {
            Item item = _itemRepository.GetItemById(itemId);
            totalWeight += item?.InitiativeCost ?? 1;
        }

        return totalWeight;
    }

    // ========== WORK AND REST OPERATIONS ==========

    /// <summary>
    /// Execute a rest action to recover resources
    /// </summary>
    public RestActionResult ExecuteRestAction(string actionType)
    {
        // Simple rest implementation - would be expanded based on rest system
        return new RestActionResult
        {
            Success = true,
            TimeAdvanced = 2, // 2 segments
            StaminaRecovered = 2,
            HealthRecovered = 1
        };
    }

    /// <summary>
    /// Perform work to earn coins with hunger-based scaling
    /// </summary>
    public WorkResult PerformWork()
    {
        // Base work payment
        int baseAmount = 8;

        // Apply hunger penalty: coins = base_amount - floor(hunger/25)
        int currentHunger = GetHunger();
        int hungerPenalty = currentHunger / 25; // Integer division = floor
        int coinsEarned = Math.Max(0, baseAmount - hungerPenalty); // Never go below 0

        // Add coins
        AddCoins(coinsEarned, "Work performed");

        // Generate message about hunger impact if any
        string hungerMessage = hungerPenalty > 0 ? $" (reduced by {hungerPenalty} due to hunger)" : "";

        _messageSystem.AddSystemMessage($"💼 Earned {coinsEarned} coins from work{hungerMessage}", SystemMessageTypes.Success);

        return new WorkResult
        {
            Success = true,
            Message = $"Earned {coinsEarned} coins from work{hungerMessage}",
            CoinsEarned = coinsEarned
        };
    }

    public bool HasItem(string itemId)
    {
        return _gameWorld.GetPlayer().Inventory.HasItem(itemId);
    }

    public int GetItemCount(string itemId)
    {
        return _gameWorld.GetPlayer().Inventory.GetItemCount(itemId);
    }

    public bool AddItem(string itemId)
    {
        return _gameWorld.GetPlayer().Inventory.AddItem(itemId);
    }

    public bool RemoveItem(string itemId)
    {
        return _gameWorld.GetPlayer().Inventory.RemoveItem(itemId);
    }
}

/// <summary>
/// Result of a rest action
/// </summary>
public class RestActionResult
{
    public bool Success { get; set; }
    public int TimeAdvanced { get; set; }
    public int StaminaRecovered { get; set; }
    public int HealthRecovered { get; set; }
}
