/// <summary>
/// Public facade for all resource-related operations.
/// Manages coins, health, hunger, and attention across the game.
/// TWO PILLARS: Delegates all mutations to RewardApplicationService
/// </summary>
public class ResourceFacade
{
    private readonly GameWorld _gameWorld;
    private readonly ResourceCalculator _resourceCalculator;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly ItemRepository _itemRepository;
    private readonly StateClearingResolver _stateClearingResolver;
    private readonly RewardApplicationService _rewardApplicationService;

    public ResourceFacade(
        GameWorld gameWorld,
        ResourceCalculator resourceCalculator,
        MessageSystem messageSystem,
        TimeManager timeManager,
        ItemRepository itemRepository,
        StateClearingResolver stateClearingResolver,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _resourceCalculator = resourceCalculator ?? throw new ArgumentNullException(nameof(resourceCalculator));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _stateClearingResolver = stateClearingResolver ?? throw new ArgumentNullException(nameof(stateClearingResolver));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
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

    public async Task<bool> SpendCoins(int amount, string reason)
    {
        Player player = _gameWorld.GetPlayer();

        // HIGHLANDER: Use CompoundRequirement for affordability check
        Consequence cost = new Consequence { Coins = -amount };
        CompoundRequirement resourceReq = CompoundRequirement.CreateForConsequence(cost);
        if (!resourceReq.IsAnySatisfied(player, _gameWorld))
        {
            _messageSystem.AddSystemMessage(
                $"Not enough coins! Need {amount}, have {player.Coins}",
                SystemMessageTypes.Warning,
                MessageCategory.ResourceChange);
            return false;
        }

        // TWO PILLARS: Apply cost via Consequence
        Consequence coinCost = new Consequence { Coins = -amount };
        await _rewardApplicationService.ApplyConsequence(coinCost, null);

        _messageSystem.AddSystemMessage(
            $"Spent {amount} coins on {reason} ({player.Coins} remaining)",
            SystemMessageTypes.Info,
            MessageCategory.ResourceChange);
        return true;
    }

    public async Task<bool> SpendCoins(int amount)
    {
        return await SpendCoins(amount, "Travel cost");
    }

    public async Task AddCoins(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();

        // TWO PILLARS: Apply reward via Consequence
        Consequence coinReward = new Consequence { Coins = amount };
        await _rewardApplicationService.ApplyConsequence(coinReward, null);

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

    public async Task TakeDamage(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHealth = player.Health;

        // TWO PILLARS: Apply damage via Consequence + ApplyConsequence
        Consequence damageConsequence = new Consequence { Health = -amount };
        await _rewardApplicationService.ApplyConsequence(damageConsequence, null);

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

    public async Task Heal(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHealth = player.Health;

        // TWO PILLARS: Apply healing via Consequence + ApplyConsequence
        Consequence healConsequence = new Consequence { Health = amount };
        await _rewardApplicationService.ApplyConsequence(healConsequence, null);

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

    public async Task IncreaseHunger(int amount, string reason)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHunger = player.Hunger;

        // TWO PILLARS: Apply hunger increase via Consequence + ApplyConsequence
        // Positive hunger value means hunger increases (bad)
        Consequence hungerConsequence = new Consequence { Hunger = amount };
        await _rewardApplicationService.ApplyConsequence(hungerConsequence, null);

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

    public async Task DecreaseHunger(int amount, string source)
    {
        Player player = _gameWorld.GetPlayer();
        int oldHunger = player.Hunger;

        // TWO PILLARS: Apply hunger decrease via Consequence + ApplyConsequence
        // Negative hunger value means hunger decreases (good)
        Consequence hungerConsequence = new Consequence { Hunger = -amount };
        await _rewardApplicationService.ApplyConsequence(hungerConsequence, null);

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
    /// HIGHLANDER: Hunger increase happens in GameOrchestrator.ProcessTimeAdvancement (one place only).
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
    /// Execute Rest action: Advance 1 time segment and restore resources based on Consequence.
    /// Hunger increases by +5 automatically via time progression.
    /// Data-driven: consequence from action definition.
    /// Now also clears states that have ClearsOnRest behavior.
    /// TWO PILLARS: Delegates resource mutations to RewardApplicationService
    /// HIGHLANDER: Consequence is the ONLY class for resource outcomes.
    /// </summary>
    public async Task ExecuteRest(Consequence consequence)
    {
        Player player = _gameWorld.GetPlayer();

        // TIME PROGRESSION: Handled by GameOrchestrator orchestration (calls TimeFacade.AdvanceSegments before this method)
        // Hunger increases by +5 per segment (automatic via time progression)
        // No need to manually modify hunger here

        // Resource recovery - data-driven from action consequence
        int healthBefore = player.Health;
        int staminaBefore = player.Stamina;

        // TWO PILLARS: Apply resource recovery via Consequence + ApplyConsequence
        await _rewardApplicationService.ApplyConsequence(consequence, null);

        int healthRecovered = player.Health - healthBefore;
        int staminaRecovered = player.Stamina - staminaBefore;

        // STATE CLEARING: Get projection of states to clear on rest
        Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
        bool isSafe = currentLocation != null && currentLocation.Safety == LocationSafety.Safe;
        List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnRest(isSafe);

        // Apply state clearing
        int statesCleared = 0;
        foreach (StateType stateType in statesToClear)
        {
            _gameWorld.ClearState(stateType);
            statesCleared++;
        }

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
    /// Consume an item and clear states based on item type.
    /// NOT IMPLEMENTED: Requires Item.ItemType property and inventory system.
    /// Will be implemented in Phase 6 when item consumption mechanics are designed.
    /// </summary>
    public bool ConsumeItem(Item item)
    {
        throw new NotImplementedException(
            "ConsumeItem() not yet implemented. " +
            "Requires: (1) Item.ItemType property, (2) Inventory system, (3) Item-to-StateClearing mapping. " +
            "Planned for Phase 6.");
    }

    /// <summary>
    /// Execute Wait action: Pass time with no resource recovery.
    /// TIME PROGRESSION: Handled by GameOrchestrator orchestration (calls TimeFacade.AdvanceSegments before this method)
    /// Hunger increases by +5 automatically via time progression.
    /// Global action available everywhere.
    /// </summary>
    public void ExecuteWait()
    {
        // TIME PROGRESSION: Handled by GameOrchestrator orchestration (calls TimeFacade.AdvanceSegments before this method)
        // Hunger increases by +5 per segment (automatic via time progression)
        // No resource recovery - just passing time

        _messageSystem.AddSystemMessage("Waited for 1 segment, passing time without activity", SystemMessageTypes.Info, MessageCategory.TimeProgression);
    }

    /// <summary>
    /// Perform work to earn coins with hunger-based scaling.
    /// Data-driven: base pay from action consequence.
    /// TWO PILLARS: Delegates coin mutation to RewardApplicationService
    /// HIGHLANDER: Consequence is the ONLY class for resource outcomes.
    /// </summary>
    public async Task<WorkResult> PerformWork(Consequence consequence)
    {
        // Base work payment - data-driven from action consequence
        // HIGHLANDER: Coins > 0 means reward (gain coins)
        int baseAmount = consequence.Coins > 0 ? consequence.Coins : 0;

        // Apply hunger penalty: coins = base_amount - floor(hunger/25)
        int currentHunger = GetHunger();
        int hungerPenalty = currentHunger / 25; // Integer division = floor
        int coinsEarned = Math.Max(0, baseAmount - hungerPenalty); // Never go below 0

        // Add coins
        await AddCoins(coinsEarned, "Work performed");

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
