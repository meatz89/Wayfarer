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
        private readonly AttentionManager _attentionManager;
        private readonly ResourceCalculator _resourceCalculator;
        private readonly MessageSystem _messageSystem;
        private readonly TimeManager _timeManager;
        
        public ResourceFacade(
            GameWorld gameWorld,
            CoinManager coinManager,
            HealthManager healthManager,
            HungerManager hungerManager,
            AttentionManager attentionManager,
            ResourceCalculator resourceCalculator,
            MessageSystem messageSystem,
            TimeManager timeManager)
        {
            _gameWorld = gameWorld;
            _coinManager = coinManager;
            _healthManager = healthManager;
            _hungerManager = hungerManager;
            _attentionManager = attentionManager;
            _resourceCalculator = resourceCalculator;
            _messageSystem = messageSystem;
            _timeManager = timeManager;
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
            var attention = _attentionManager.GetAttentionForTimeBlock(_gameWorld.GetPlayer(), timeBlock);
            return new AttentionInfo(attention.Current, attention.Max);
        }
        
        public bool CanAffordAttention(int amount, TimeBlocks timeBlock)
        {
            return _attentionManager.CanAfford(_gameWorld.GetPlayer(), amount, timeBlock);
        }
        
        public bool SpendAttention(int amount, TimeBlocks timeBlock, string reason)
        {
            return _attentionManager.SpendAttention(_gameWorld.GetPlayer(), amount, timeBlock, reason, _messageSystem);
        }
        
        public bool SpendAttention(int amount)
        {
            var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            return SpendAttention(amount, currentTimeBlock, "Observation");
        }
        
        public void RefreshAttentionForNewTimeBlock(TimeBlocks newTimeBlock)
        {
            int hunger = GetHunger();
            int maxAttention = _resourceCalculator.CalculateMorningAttention(hunger);
            _attentionManager.RefreshForTimeBlock(_gameWorld.GetPlayer(), newTimeBlock, maxAttention, _messageSystem);
        }
        
        // ========== RESOURCE CALCULATIONS ==========
        
        public int CalculateCarryCapacity()
        {
            int health = GetHealth();
            return _resourceCalculator.CalculateWeightLimit(health);
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
        public (int Coins, int Health, int Hunger, int CurrentAttention, int MaxAttention) GetPlayerResources()
        {
            var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            var (currentAttention, maxAttention) = GetAttention(currentTimeBlock);
            
            return (
                Coins: GetCoins(),
                Health: GetHealth(),
                Hunger: GetHunger(),
                CurrentAttention: currentAttention,
                MaxAttention: maxAttention
            );
        }
        
        // ========== INVENTORY OPERATIONS (delegated from Player) ==========
        
        public Inventory GetInventory()
        {
            return _gameWorld.GetPlayer().Inventory;
        }
        
        public InventoryViewModel GetInventoryViewModel()
        {
            var inventory = GetInventory();
            return new InventoryViewModel
            {
                Items = inventory.GetItemIds().Select(itemId => new InventoryItemViewModel
                {
                    ItemId = itemId,
                    Name = itemId, // TODO: Get actual item name from repository
                    Description = "", // TODO: Get actual item description from repository
                    Weight = 1, // TODO: Get actual item weight from repository
                    Value = 0, // TODO: Get actual item value from repository
                    CanRead = false
                }).ToList(),
                TotalWeight = CalculateTotalWeight(),
                MaxSlots = inventory.GetCapacity(),
                UsedSlots = inventory.UsedCapacity,
                Coins = GetCoins()
            };
        }
        
        public int CalculateTotalWeight()
        {
            // TODO: Get actual item weights from repository
            return GetInventory().UsedCapacity; // Simplified - 1 weight per item
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
            var coinsEarned = 5;
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