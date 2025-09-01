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
        
        public (int Current, int Max) GetAttention(TimeBlocks timeBlock)
        {
            return _attentionManager.GetAttentionForTimeBlock(_gameWorld.GetPlayer(), timeBlock);
        }
        
        public bool CanAffordAttention(int amount, TimeBlocks timeBlock)
        {
            return _attentionManager.CanAfford(_gameWorld.GetPlayer(), amount, timeBlock);
        }
        
        public bool SpendAttention(int amount, TimeBlocks timeBlock, string reason)
        {
            return _attentionManager.SpendAttention(_gameWorld.GetPlayer(), amount, timeBlock, reason, _messageSystem);
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
        
        // ========== INVENTORY OPERATIONS (delegated from Player) ==========
        
        public Inventory GetInventory()
        {
            return _gameWorld.GetPlayer().Inventory;
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
}