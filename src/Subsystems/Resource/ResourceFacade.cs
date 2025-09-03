using System;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
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
        private readonly TimeBlockAttentionManager _attentionManager;
        private readonly ResourceCalculator _resourceCalculator;
        private readonly MessageSystem _messageSystem;
        private readonly TimeManager _timeManager;
        private readonly ItemRepository _itemRepository;

        public ResourceFacade(
            GameWorld gameWorld,
            CoinManager coinManager,
            HealthManager healthManager,
            HungerManager hungerManager,
            TimeBlockAttentionManager attentionManager,
            ResourceCalculator resourceCalculator,
            MessageSystem messageSystem,
            TimeManager timeManager,
            ItemRepository itemRepository)
        {
            _gameWorld = gameWorld;
            _coinManager = coinManager;
            _healthManager = healthManager;
            _hungerManager = hungerManager;
            _attentionManager = attentionManager;
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

        // ========== ATTENTION OPERATIONS ==========

        public AttentionInfo GetAttention(TimeBlocks timeBlock)
        {
            AttentionInfo attentionState = _attentionManager.GetAttentionState();
            return new AttentionInfo(attentionState.Current, attentionState.Max);
        }

        public bool CanAffordAttention(int amount, TimeBlocks timeBlock)
        {
            return _attentionManager.HasAttentionRemaining() && _attentionManager.GetAttentionState().Current >= amount;
        }

        public bool SpendAttention(int amount, TimeBlocks timeBlock, string reason)
        {
            AttentionInfo currentAttention = _attentionManager.GetAttentionState();
            if (currentAttention.Current >= amount)
            {
                // TimeBlockAttentionManager uses internal AttentionManager, need to access it
                var internalAttention = _attentionManager.GetCurrentAttention(timeBlock);
                bool success = internalAttention.TrySpend(amount);
                if (success)
                {
                    _messageSystem.AddSystemMessage(
                        $"🧠 Spent {amount} attention on {reason} ({currentAttention.Current - amount}/{currentAttention.Max} remaining)",
                        SystemMessageTypes.Info);
                }
                return success;
            }
            
            _messageSystem.AddSystemMessage(
                $"🧠 Not enough attention! Need {amount}, have {currentAttention.Current}",
                SystemMessageTypes.Warning);
            return false;
        }

        public bool SpendAttention(int amount)
        {
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            return SpendAttention(amount, currentTimeBlock, "Observation");
        }

        public void RefreshAttentionForNewTimeBlock(TimeBlocks newTimeBlock)
        {
            int hunger = GetHunger();
            int maxAttention = _resourceCalculator.CalculateMorningAttention(hunger);
            _attentionManager.ForceRefresh();
            _messageSystem.AddSystemMessage(
                $"🧠 Attention refreshed for {newTimeBlock}: {maxAttention} points available",
                SystemMessageTypes.Success);
        }

        // ========== RESOURCE CALCULATIONS ==========

        public int CalculateCarryCapacity()
        {
            int health = GetHealth();
            return _resourceCalculator.CalculateFocusLimit(health);
        }

        public int CalculateMorningAttention()
        {
            int hunger = GetHunger();
            return _resourceCalculator.CalculateMorningAttention(hunger);
        }

        public void ProcessTimeBlockTransition(TimeBlocks oldBlock, TimeBlocks newBlock)
        {
            // Increase hunger by 20 per time period
            IncreaseHunger(20, "Time passes");

            // Refresh attention for new time block
            RefreshAttentionForNewTimeBlock(newBlock);
        }

        // ========== COMBINED RESOURCE OPERATIONS ==========

        /// <summary>
        /// Get complete player resources for UI display
        /// </summary>
        public PlayerResourcesInfo GetPlayerResources()
        {
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            AttentionInfo attentionInfo = GetAttention(currentTimeBlock);

            return new PlayerResourcesInfo(
                coins: GetCoins(),
                health: GetHealth(),
                hunger: GetHunger(),
                currentAttention: attentionInfo.Current,
                maxAttention: attentionInfo.Max
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
                        Focus = item?.Focus ?? 1,
                        Value = item?.SellPrice ?? 0,
                        CanRead = item?.Categories.Contains(ItemCategory.Special_Document) ?? false
                    };
                }).ToList(),
                TotalFocus = CalculateTotalFocus(),
                MaxSlots = inventory.GetCapacity(),
                UsedSlots = inventory.UsedCapacity,
                Coins = GetCoins()
            };
        }

        public int CalculateTotalFocus()
        {
            Inventory inventory = GetInventory();
            int totalFocus = 0;

            foreach (string itemId in inventory.GetItemIds())
            {
                Item item = _itemRepository.GetItemById(itemId);
                totalFocus += item?.Focus ?? 1;
            }

            return totalFocus;
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
                TimeAdvanced = 60, // 1 hour
                StaminaRecovered = 2,
                HealthRecovered = 1
            };
        }

        /// <summary>
        /// Perform work to earn coins
        /// </summary>
        public WorkResult PerformWork()
        {
            // Simple work implementation
            int coinsEarned = 5;
            AddCoins(coinsEarned, "Work performed");

            return new WorkResult
            {
                Success = true,
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
}