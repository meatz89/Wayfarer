using System;

/// <summary>
/// Public facade for all resource-related operations.
/// Manages coins, health, hunger, and attention across the game.
/// </summary>
public class ResourceFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ResourceCalculator _resourceCalculator;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly ItemRepository _itemRepository;

    public ResourceFacade(
        GameWorld gameWorld,
        ResourceCalculator resourceCalculator,
        MessageSystem messageSystem,
        TimeManager timeManager,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _resourceCalculator = resourceCalculator ?? throw new ArgumentNullException(nameof(resourceCalculator));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
    }

    // ========== COIN OPERATIONS ==========

    public int GetCoins()
    {
        return _gameWorld.GetPlayer().Coins;
    }

    public bool CanAfford(int amount)
    {
        return _gameWorld.GetPlayer().Coins >= amount;
    }

    public bool SpendCoins(int amount, string reason)
    {
        Player player = _gameWorld.GetPlayer();
        if (player.Coins < amount)
        {
            _messageSystem.AddSystemMessage(
                $"💰 Not enough coins! Need {amount}, have {player.Coins}",
                SystemMessageTypes.Warning);
            return false;
        }

        player.Coins -= amount;
        _messageSystem.AddSystemMessage(
            $"💰 Spent {amount} coins on {reason} ({player.Coins} remaining)",
            SystemMessageTypes.Info);
        return true;
    }

    public bool SpendCoins(int amount)
    {
        return SpendCoins(amount, "Travel cost");
    }

    public void AddCoins(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        player.Coins += amount;
        _messageSystem.AddSystemMessage(
            $"💰 Received {amount} coins from {source} (total: {player.Coins})",
            SystemMessageTypes.Success);
    }

    // ========== HEALTH OPERATIONS ==========

    public int GetHealth()
    {
        return _gameWorld.GetPlayer().Health;
    }

    public void TakeDamage(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHealth = player.Health;
        player.Health = Math.Max(0, player.Health - amount);
        int actualDamage = oldHealth - player.Health;

        if (actualDamage > 0)
        {
            _messageSystem.AddSystemMessage(
                $"❤️ Took {actualDamage} damage from {source} (Health: {player.Health}/{Player.MaxHealthConstant})",
                SystemMessageTypes.Warning);

            if (player.Health == 0)
            {
                _messageSystem.AddSystemMessage(
                    "💀 You have died!",
                    SystemMessageTypes.Warning);
            }
        }
    }

    public void Heal(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHealth = player.Health;
        player.Health = Math.Min(Player.MaxHealthConstant, player.Health + amount);
        int actualHealing = player.Health - oldHealth;

        if (actualHealing > 0)
        {
            _messageSystem.AddSystemMessage(
                $"❤️ Healed {actualHealing} from {source} (Health: {player.Health}/{Player.MaxHealthConstant})",
                SystemMessageTypes.Success);
        }
    }

    public bool IsAlive()
    {
        return _gameWorld.GetPlayer().Health > 0;
    }

    // ========== HUNGER OPERATIONS ==========

    public int GetHunger()
    {
        return _gameWorld.GetPlayer().Hunger;
    }

    public void IncreaseHunger(int amount, string reason)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHunger = player.Hunger;
        player.Hunger = Math.Min(Player.MaxHungerConstant, player.Hunger + amount);
        int actualIncrease = player.Hunger - oldHunger;

        if (actualIncrease > 0)
        {
            string hungerLevel = player.GetHungerLevelDescription();
            _messageSystem.AddSystemMessage(
                $"🍞 Hunger increased by {actualIncrease} - {reason} ({player.Hunger}/{Player.MaxHungerConstant} - {hungerLevel})",
                SystemMessageTypes.Info);

            if (player.IsStarving())
            {
                _messageSystem.AddSystemMessage(
                    "⚠️ You are starving! Find food soon!",
                    SystemMessageTypes.Warning);
            }
        }
    }

    public void DecreaseHunger(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHunger = player.Hunger;
        player.Hunger = Math.Max(0, player.Hunger - amount);
        int actualDecrease = oldHunger - player.Hunger;

        if (actualDecrease > 0)
        {
            string hungerLevel = player.GetHungerLevelDescription();
            _messageSystem.AddSystemMessage(
                $"🍖 Ate {source}, hunger reduced by {actualDecrease} ({player.Hunger}/{Player.MaxHungerConstant} - {hungerLevel})",
                SystemMessageTypes.Success);
        }
    }

    public bool IsStarving()
    {
        return _gameWorld.GetPlayer().IsStarving();
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

        // Apply daily NPC decay when transitioning to Morning (morning refresh)
        if (newBlock == TimeBlocks.Morning)
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
                Item item = _itemRepository.GetItemById(itemId);
                if (item == null)
                {
                    throw new InvalidOperationException($"Item {itemId} not found in repository");
                }

                return new InventoryItemViewModel
                {
                    ItemId = itemId,
                    Name = item.Name,
                    Description = item.Description,
                    Weight = item.InitiativeCost,
                    Value = item.SellPrice,
                    CanRead = item.Categories.Contains(ItemCategory.Special_Document)
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
            if (item == null)
            {
                throw new InvalidOperationException($"Item {itemId} not found in repository");
            }

            totalWeight += item.InitiativeCost;
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
