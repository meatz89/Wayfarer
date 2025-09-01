using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
    /// <summary>
    /// Manages attention resources per time block.
    /// Attention refreshes at the start of each time block based on hunger.
    /// </summary>
    public class AttentionManager
    {
        private Dictionary<TimeBlocks, (int Current, int Max)> _attentionByTimeBlock = new Dictionary<TimeBlocks, (int, int)>();
        
        public (int Current, int Max) GetAttentionForTimeBlock(Player player, TimeBlocks timeBlock)
        {
            if (!_attentionByTimeBlock.ContainsKey(timeBlock))
            {
                // Initialize with default values if not set
                return (0, 0);
            }
            return _attentionByTimeBlock[timeBlock];
        }
        
        public bool CanAfford(Player player, int amount, TimeBlocks timeBlock)
        {
            var (current, max) = GetAttentionForTimeBlock(player, timeBlock);
            return current >= amount;
        }
        
        public bool SpendAttention(Player player, int amount, TimeBlocks timeBlock, string reason, MessageSystem messageSystem)
        {
            if (!CanAfford(player, amount, timeBlock))
            {
                messageSystem.AddSystemMessage(
                    $"🧠 Not enough attention! Need {amount}, have {GetAttentionForTimeBlock(player, timeBlock).Current}",
                    SystemMessageTypes.Warning);
                return false;
            }
            
            var (current, max) = _attentionByTimeBlock[timeBlock];
            _attentionByTimeBlock[timeBlock] = (current - amount, max);
            
            messageSystem.AddSystemMessage(
                $"🧠 Spent {amount} attention on {reason} ({current - amount}/{max} remaining)",
                SystemMessageTypes.Info);
            
            Console.WriteLine($"[AttentionManager] Spent {amount} attention on {reason} during {timeBlock}. Remaining: {current - amount}/{max}");
            return true;
        }
        
        public void RefreshForTimeBlock(Player player, TimeBlocks timeBlock, int maxAttention, MessageSystem messageSystem)
        {
            _attentionByTimeBlock[timeBlock] = (maxAttention, maxAttention);
            
            messageSystem.AddSystemMessage(
                $"🧠 Attention refreshed for {timeBlock}: {maxAttention} points available",
                SystemMessageTypes.Success);
            
            Console.WriteLine($"[AttentionManager] Attention refreshed for {timeBlock}: {maxAttention}/{maxAttention}");
        }
        
        public void RestoreAttention(Player player, int amount, TimeBlocks timeBlock, string source, MessageSystem messageSystem)
        {
            if (!_attentionByTimeBlock.ContainsKey(timeBlock))
            {
                Console.WriteLine($"[AttentionManager] Warning: No attention data for {timeBlock}");
                return;
            }
            
            var (current, max) = _attentionByTimeBlock[timeBlock];
            int newCurrent = Math.Min(max, current + amount);
            int actualRestore = newCurrent - current;
            
            if (actualRestore > 0)
            {
                _attentionByTimeBlock[timeBlock] = (newCurrent, max);
                messageSystem.AddSystemMessage(
                    $"🧠 Restored {actualRestore} attention from {source} ({newCurrent}/{max})",
                    SystemMessageTypes.Success);
                
                Console.WriteLine($"[AttentionManager] Restored {actualRestore} attention from {source}. Current: {newCurrent}/{max}");
            }
        }
    }
}