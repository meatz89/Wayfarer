using System;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
    /// <summary>
    /// Manages player health and damage/healing operations.
    /// </summary>
    public class HealthManager
    {
        private const int MAX_HEALTH = 100;
        private const int MIN_HEALTH = 0;

        public int GetCurrentHealth(Player player)
        {
            return player.Health;
        }

        public int GetMaxHealth()
        {
            return MAX_HEALTH;
        }

        public bool IsAlive(Player player)
        {
            return player.Health > MIN_HEALTH;
        }

        public bool IsInjured(Player player)
        {
            return player.Health < MAX_HEALTH;
        }

        public void TakeDamage(Player player, int amount, string source, MessageSystem messageSystem)
        {
            int oldHealth = player.Health;
            player.Health = Math.Max(MIN_HEALTH, player.Health - amount);
            int actualDamage = oldHealth - player.Health;

            if (actualDamage > 0)
            {
                messageSystem.AddSystemMessage(
                    $"❤️ Took {actualDamage} damage from {source} (Health: {player.Health}/{MAX_HEALTH})",
                    SystemMessageTypes.Warning);

                if (!IsAlive(player))
                {
                    messageSystem.AddSystemMessage(
                        "💀 You have died!",
                        SystemMessageTypes.Warning);
                }
            }

            Console.WriteLine($"[HealthManager] Player took {actualDamage} damage from {source}. Health: {player.Health}/{MAX_HEALTH}");
        }

        public void Heal(Player player, int amount, string source, MessageSystem messageSystem)
        {
            int oldHealth = player.Health;
            player.Health = Math.Min(MAX_HEALTH, player.Health + amount);
            int actualHealing = player.Health - oldHealth;

            if (actualHealing > 0)
            {
                messageSystem.AddSystemMessage(
                    $"❤️ Healed {actualHealing} from {source} (Health: {player.Health}/{MAX_HEALTH})",
                    SystemMessageTypes.Success);
            }

            Console.WriteLine($"[HealthManager] Player healed {actualHealing} from {source}. Health: {player.Health}/{MAX_HEALTH}");
        }

    }
}