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
        private readonly ItemRepository _itemRepository;
        
        public ResourceFacade(
            GameWorld gameWorld,
            CoinManager coinManager,
            HealthManager healthManager,
            HungerManager hungerManager,
            AttentionManager attentionManager,
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
        public PlayerResourcesInfo GetPlayerResources()
        {
            var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            var attentionInfo = GetAttention(currentTimeBlock);
            
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
            var inventory = GetInventory();
            return new InventoryViewModel
            {
                Items = inventory.GetItemIds().Select(itemId => 
                {
                    var item = _itemRepository.GetItemById(itemId);
                    return new InventoryItemViewModel
                    {
                        ItemId = itemId,
                        Name = item?.Name ?? itemId,
                        Description = item?.Description ?? "",
                        Weight = item?.Weight ?? 1,
                        Value = item?.SellPrice ?? 0,
                        CanRead = item?.Categories.Contains(ItemCategory.Special_Document) ?? false
                    };
                }).ToList(),
                TotalWeight = CalculateTotalWeight(),
                MaxSlots = inventory.GetCapacity(),
                UsedSlots = inventory.UsedCapacity,
                Coins = GetCoins()
            };
        }
        
        public int CalculateTotalWeight()
        {
            var inventory = GetInventory();
            var totalWeight = 0;
            
            foreach (var itemId in inventory.GetItemIds())
            {
                var item = _itemRepository.GetItemById(itemId);
                totalWeight += item?.Weight ?? 1;
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