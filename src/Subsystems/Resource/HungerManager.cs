using System;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
    /// <summary>
    /// Manages player hunger and food consumption.
    /// </summary>
    public class HungerManager
    {
        private const int MAX_HUNGER = 100;
        private const int MIN_HUNGER = 0;
        private const int STARVING_THRESHOLD = 80;
        private const int HUNGRY_THRESHOLD = 50;
        private const int HUNGER_PER_TIME_BLOCK = 20;
        
        public int GetCurrentHunger(Player player)
        {
            return player.Food;
        }
        
        public int GetMaxHunger()
        {
            return MAX_HUNGER;
        }
        
        public bool IsStarving(Player player)
        {
            return player.Food >= STARVING_THRESHOLD;
        }
        
        public bool IsHungry(Player player)
        {
            return player.Food >= HUNGRY_THRESHOLD;
        }
        
        public void IncreaseHunger(Player player, int amount, string reason, MessageSystem messageSystem)
        {
            int oldHunger = player.Food;
            player.Food = Math.Min(MAX_HUNGER, player.Food + amount);
            int actualIncrease = player.Food - oldHunger;
            
            if (actualIncrease > 0)
            {
                string hungerLevel = GetHungerLevelDescription(player);
                messageSystem.AddSystemMessage(
                    $"🍞 Hunger increased by {actualIncrease} - {reason} ({player.Food}/{MAX_HUNGER} - {hungerLevel})",
                    SystemMessageTypes.Info);
                
                if (IsStarving(player))
                {
                    messageSystem.AddSystemMessage(
                        "⚠️ You are starving! Find food soon!",
                        SystemMessageTypes.Warning);
                }
            }
            
            Console.WriteLine($"[HungerManager] Hunger increased by {actualIncrease} - {reason}. Current: {player.Food}/{MAX_HUNGER}");
        }
        
        public void DecreaseHunger(Player player, int amount, string source, MessageSystem messageSystem)
        {
            int oldHunger = player.Food;
            player.Food = Math.Max(MIN_HUNGER, player.Food - amount);
            int actualDecrease = oldHunger - player.Food;
            
            if (actualDecrease > 0)
            {
                string hungerLevel = GetHungerLevelDescription(player);
                messageSystem.AddSystemMessage(
                    $"🍖 Ate {source}, hunger reduced by {actualDecrease} ({player.Food}/{MAX_HUNGER} - {hungerLevel})",
                    SystemMessageTypes.Success);
            }
            
            Console.WriteLine($"[HungerManager] Hunger decreased by {actualDecrease} from {source}. Current: {player.Food}/{MAX_HUNGER}");
        }
        
        public void ProcessTimeBlockHunger(Player player, MessageSystem messageSystem)
        {
            IncreaseHunger(player, HUNGER_PER_TIME_BLOCK, "time passes", messageSystem);
        }
        
        private string GetHungerLevelDescription(Player player)
        {
            if (IsStarving(player)) return "Starving";
            if (IsHungry(player)) return "Hungry";
            if (player.Food >= 30) return "Peckish";
            return "Satisfied";
        }
    }
}