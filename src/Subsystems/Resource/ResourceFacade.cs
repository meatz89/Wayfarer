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
    private readonly StateClearingResolver _stateClearingResolver;

    public ResourceFacade(
        GameWorld gameWorld,
        ResourceCalculator resourceCalculator,
        MessageSystem messageSystem,
        TimeManager timeManager,
        ItemRepository itemRepository,
        StateClearingResolver stateClearingResolver)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _resourceCalculator = resourceCalculator ?? throw new ArgumentNullException(nameof(resourceCalculator));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _stateClearingResolver = stateClearingResolver ?? throw new ArgumentNullException(nameof(stateClearingResolver));
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
                $"Not enough coins! Need {amount}, have {player.Coins}",
                SystemMessageTypes.Warning,
                MessageCategory.ResourceChange);
            return false;
        }

        player.Coins -= amount;
        _messageSystem.AddSystemMessage(
            $"Spent {amount} coins on {reason} ({player.Coins} remaining)",
            SystemMessageTypes.Info,
            MessageCategory.ResourceChange);
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
            $"Received {amount} coins from {source} (total: {player.Coins})",
            SystemMessageTypes.Success,
            MessageCategory.ResourceChange);
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
                $"Took {actualDamage} damage from {source} (Health: {player.Health}/{player.MaxHealth})",
                SystemMessageTypes.Warning,
                MessageCategory.ResourceChange);

            if (player.Health == 0)
            {
                _messageSystem.AddSystemMessage(
                    "You have died!",
                    SystemMessageTypes.Warning,
                    MessageCategory.Danger);
            }
        }
    }

    public void Heal(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHealth = player.Health;
        player.Health = Math.Min(player.MaxHealth, player.Health + amount);
        int actualHealing = player.Health - oldHealth;

        if (actualHealing > 0)
        {
            _messageSystem.AddSystemMessage(
                $"Healed {actualHealing} from {source} (Health: {player.Health}/{player.MaxHealth})",
                SystemMessageTypes.Success,
                MessageCategory.ResourceChange);
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
        player.Hunger = Math.Min(player.MaxHunger, player.Hunger + amount);
        int actualIncrease = player.Hunger - oldHunger;

        if (actualIncrease > 0)
        {
            string hungerLevel = player.GetHungerLevelDescription();
            _messageSystem.AddSystemMessage(
                $"Hunger increased by {actualIncrease} - {reason} ({player.Hunger}/{player.MaxHunger} - {hungerLevel})",
                SystemMessageTypes.Info,
                MessageCategory.ResourceChange);

            if (player.IsStarving())
            {
                _messageSystem.AddSystemMessage(
                    "You are starving! Find food soon!",
                    SystemMessageTypes.Warning,
                    MessageCategory.Danger);
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
                $"Ate {source}, hunger reduced by {actualDecrease} ({player.Hunger}/{player.MaxHunger} - {hungerLevel})",
                SystemMessageTypes.Success,
                MessageCategory.ResourceChange);
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

    /// <summary>
    /// Process day transition effects (NPC decay at dawn).
    /// ONLY called when transitioning to Morning (new day starts).
    /// HIGHLANDER: Hunger increase happens in GameFacade.ProcessTimeAdvancement (one place only).
    /// </summary>
    public void ProcessDayTransition()
    {
        // Apply daily NPC decay at dawn
        ApplyNPCDailyDecay();
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
            "A new day dawns. Relationships naturally shift toward neutral over time.",
            SystemMessageTypes.Info,
            MessageCategory.TimeProgression);
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
            Items = inventory.GetAllItems().Select(item =>
            {
                return new InventoryItemViewModel
                {
                    Item = item, // HIGHLANDER: Object reference, not ID
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

        foreach (Item item in inventory.GetAllItems())
        {
            totalWeight += item.InitiativeCost;
        }

        return totalWeight;
    }

    // ========== WORK AND REST OPERATIONS ==========

    /// <summary>
    /// Execute Rest action: Advance 1 time segment and restore resources based on action rewards.
    /// Hunger increases by +5 automatically via time progression.
    /// Data-driven: rewards from JSON action definition.
    /// Now also clears states that have ClearsOnRest behavior.
    /// </summary>
    public void ExecuteRest(ActionRewards rewards)
    {
        Player player = _gameWorld.GetPlayer();

        // TIME PROGRESSION: Handled by GameFacade orchestration (calls TimeFacade.AdvanceSegments before this method)
        // Hunger increases by +5 per segment (automatic via time progression)
        // No need to manually modify hunger here

        // Resource recovery - data-driven from action rewards
        int healthBefore = player.Health;
        int staminaBefore = player.Stamina;

        player.Health = Math.Min(player.Health + rewards.HealthRecovery, player.MaxHealth);
        player.Stamina = Math.Min(player.Stamina + rewards.StaminaRecovery, player.MaxStamina);

        int healthRecovered = player.Health - healthBefore;
        int staminaRecovered = player.Stamina - staminaBefore;

        // STATE CLEARING: Get projection of states to clear on rest
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        bool isSafe = currentLocation != null && currentLocation.Capabilities.HasFlag(LocationCapability.Guarded);
        List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnRest(isSafe);

        // Apply state clearing
        int statesCleared = 0;
        foreach (StateType stateType in statesToClear)
        {
            _gameWorld.ClearState(stateType);
            statesCleared++;
        }

        // TODO Phase 6: Trigger cascade after clearing states
        // if (statesToClear.Any())
        // {
        //     await _spawnFacade.EvaluateDormantSituations();
        // }

        // Generate message about recovery
        if (healthRecovered > 0 || staminaRecovered > 0 || statesCleared > 0)
        {
            string recoveryMessage = "Rested for 1 segment.";
            if (healthRecovered > 0) recoveryMessage += $" Health +{healthRecovered}";
            if (staminaRecovered > 0) recoveryMessage += $" Stamina +{staminaRecovered}";
            if (statesCleared > 0) recoveryMessage += $" Cleared {statesCleared} state{(statesCleared == 1 ? "" : "s")}";
            _messageSystem.AddSystemMessage(recoveryMessage, SystemMessageTypes.Success, MessageCategory.ResourceChange);
        }
        else
        {
            _messageSystem.AddSystemMessage("Rested for 1 segment (already at full health/stamina)", SystemMessageTypes.Info, MessageCategory.ResourceChange);
        }
    }

    /// <summary>
    /// Consume an item and clear states based on item type
    /// TODO: Requires Item entity to have ItemType property (Medical, Food, Remedy, Provisions)
    /// </summary>
    public bool ConsumeItem(Item item)
    {
        Player player = _gameWorld.GetPlayer();

        // TODO: Verify item exists in inventory
        // if (!player.Inventory.Contains(item)) return false;

        // TODO: Determine ItemType from item
        // This requires Item entity to have ItemType property
        // For now, we can map ItemCategory to ItemType:
        // - ItemCategory.Medicine → ItemType.Medical
        // - ItemCategory.Hunger → ItemType.Food
        // - etc.

        // TODO: Remove item from inventory
        // player.Inventory.Remove(item);

        // TODO: Get projection of states to clear
        // ItemType itemType = DetermineItemType(item);
        // List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnItemConsumption(itemType);

        // TODO: Apply state clearing
        // foreach (StateType stateType in statesToClear)
        // {
        //     _gameWorld.ClearState(stateType);
        // }

        // TODO Phase 6: Trigger cascade
        // if (statesToClear.Any())
        // {
        //     await _spawnFacade.EvaluateDormantSituations();
        // }

        // TODO: Add system message
        // _messageSystem.AddSystemMessage($"Consumed {item.Name}, cleared {statesToClear.Count} states", SystemMessageTypes.Success);

        return false; // Stub - not yet implemented
    }

    /// <summary>
    /// Execute Wait action: Pass time with no resource recovery.
    /// TIME PROGRESSION: Handled by GameFacade orchestration (calls TimeFacade.AdvanceSegments before this method)
    /// Hunger increases by +5 automatically via time progression.
    /// Global action available everywhere.
    /// </summary>
    public void ExecuteWait()
    {
        // TIME PROGRESSION: Handled by GameFacade orchestration (calls TimeFacade.AdvanceSegments before this method)
        // Hunger increases by +5 per segment (automatic via time progression)
        // No resource recovery - just passing time

        _messageSystem.AddSystemMessage("Waited for 1 segment, passing time without activity", SystemMessageTypes.Info, MessageCategory.TimeProgression);
    }

    /// <summary>
    /// Perform work to earn coins with hunger-based scaling.
    /// Data-driven: base pay from action rewards JSON.
    /// </summary>
    public WorkResult PerformWork(ActionRewards rewards)
    {
        // Base work payment - data-driven from action rewards
        int baseAmount = rewards.CoinReward;

        // Apply hunger penalty: coins = base_amount - floor(hunger/25)
        int currentHunger = GetHunger();
        int hungerPenalty = currentHunger / 25; // Integer division = floor
        int coinsEarned = Math.Max(0, baseAmount - hungerPenalty); // Never go below 0

        // Add coins
        AddCoins(coinsEarned, "Work performed");

        // Generate message about hunger impact if any
        string hungerMessage = hungerPenalty > 0 ? $" (reduced by {hungerPenalty} due to hunger)" : "";

        _messageSystem.AddSystemMessage($"Earned {coinsEarned} coins from work{hungerMessage}", SystemMessageTypes.Success, MessageCategory.ResourceChange);

        return new WorkResult
        {
            Success = true,
            Message = $"Earned {coinsEarned} coins from work{hungerMessage}",
            CoinsEarned = coinsEarned
        };
    }

    /// <summary>
    /// Check if player has specific item
    /// HIGHLANDER: Accept Item object, not string ID
    /// </summary>
    public bool HasItem(Item item)
    {
        return _gameWorld.GetPlayer().Inventory.Contains(item);
    }

    /// <summary>
    /// Get count of specific item in inventory
    /// HIGHLANDER: Accept Item object, not string ID
    /// </summary>
    public int GetItemCount(Item item)
    {
        return _gameWorld.GetPlayer().Inventory.Count(item);
    }

    /// <summary>
    /// Add item to player inventory
    /// HIGHLANDER: Accept Item object, not string ID
    /// </summary>
    public bool AddItem(Item item)
    {
        _gameWorld.GetPlayer().Inventory.Add(item);
        return true;
    }

    /// <summary>
    /// Remove item from player inventory
    /// HIGHLANDER: Accept Item object, not string ID
    /// </summary>
    public bool RemoveItem(Item item)
    {
        return _gameWorld.GetPlayer().Inventory.Remove(item);
    }
}
