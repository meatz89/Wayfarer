using System;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
    /// <summary>
    /// Manages all coin-related operations for the player.
    /// </summary>
    public class CoinManager
    {
        public int GetCurrentCoins(Player player)
        {
            return player.Coins;
        }

        public bool CanAfford(Player player, int amount)
        {
            return GetCurrentCoins(player) >= amount;
        }

        public bool SpendCoins(Player player, int amount, string reason, MessageSystem messageSystem)
        {
            if (!CanAfford(player, amount))
            {
                messageSystem.AddSystemMessage(
                    $"💰 Not enough coins! Need {amount}, have {GetCurrentCoins(player)}",
                    SystemMessageTypes.Warning);
                return false;
            }

            player.Coins -= amount;

            messageSystem.AddSystemMessage(
                $"💰 Spent {amount} coins on {reason} ({GetCurrentCoins(player)} remaining)",
                SystemMessageTypes.Info);

            Console.WriteLine($"[CoinManager] Spent {amount} coins on {reason}. Remaining: {GetCurrentCoins(player)}");
            return true;
        }

        public void AddCoins(Player player, int amount, string source, MessageSystem messageSystem)
        {
            player.Coins += amount;

            messageSystem.AddSystemMessage(
                $"💰 Received {amount} coins from {source} (total: {GetCurrentCoins(player)})",
                SystemMessageTypes.Success);

            Console.WriteLine($"[CoinManager] Added {amount} coins from {source}. Total: {GetCurrentCoins(player)}");
        }

    }
}