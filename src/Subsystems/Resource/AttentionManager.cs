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
        private Dictionary<TimeBlocks, AttentionInfo> _attentionByTimeBlock = new Dictionary<TimeBlocks, AttentionInfo>();
        
        public AttentionInfo GetAttentionForTimeBlock(Player player, TimeBlocks timeBlock)
        {
            if (!_attentionByTimeBlock.ContainsKey(timeBlock))
            {
                // Initialize with default values if not set
                return new AttentionInfo(0, 0);
            }
            return _attentionByTimeBlock[timeBlock];
        }
        
        public bool CanAfford(Player player, int amount, TimeBlocks timeBlock)
        {
            var attention = GetAttentionForTimeBlock(player, timeBlock);
            return attention.Current >= amount;
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
            
            var attention = _attentionByTimeBlock[timeBlock];
            _attentionByTimeBlock[timeBlock] = new AttentionInfo(attention.Current - amount, attention.Max);
            
            messageSystem.AddSystemMessage(
                $"🧠 Spent {amount} attention on {reason} ({attention.Current - amount}/{attention.Max} remaining)",
                SystemMessageTypes.Info);
            
            Console.WriteLine($"[AttentionManager] Spent {amount} attention on {reason} during {timeBlock}. Remaining: {attention.Current - amount}/{attention.Max}");
            return true;
        }
        
        public void RefreshForTimeBlock(Player player, TimeBlocks timeBlock, int maxAttention, MessageSystem messageSystem)
        {
            _attentionByTimeBlock[timeBlock] = new AttentionInfo(maxAttention, maxAttention);
            
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
            
            var attention = _attentionByTimeBlock[timeBlock];
            int newCurrent = Math.Min(attention.Max, attention.Current + amount);
            int actualRestore = newCurrent - attention.Current;
            
            if (actualRestore > 0)
            {
                _attentionByTimeBlock[timeBlock] = new AttentionInfo(newCurrent, attention.Max);
                messageSystem.AddSystemMessage(
                    $"🧠 Restored {actualRestore} attention from {source} ({newCurrent}/{attention.Max})",
                    SystemMessageTypes.Success);
                
                Console.WriteLine($"[AttentionManager] Restored {actualRestore} attention from {source}. Current: {newCurrent}/{attention.Max}");
            }
        }
    }
}