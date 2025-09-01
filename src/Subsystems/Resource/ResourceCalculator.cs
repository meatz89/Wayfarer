using System;

namespace Wayfarer.Subsystems.ResourceSubsystem
{
    /// <summary>
    /// Calculates resource formulas and interdependencies.
    /// Central location for all resource calculation logic.
    /// </summary>
    public class ResourceCalculator
    {
        private const int BASE_ATTENTION = 10;
        private const int MIN_ATTENTION = 2;
        private const int HUNGER_ATTENTION_DIVISOR = 25;
        
        private const int INJURED_WEIGHT_PENALTY = -1;
        private const int HEALTH_THRESHOLD_FOR_CARRY = 50;
        
        /// <summary>
        /// Calculate morning attention based on hunger level.
        /// Higher hunger = less attention available.
        /// </summary>
        public int CalculateMorningAttention(int hunger)
        {
            // Base 10 attention, reduced by hunger/25
            // Minimum 2 attention even when starving
            int reduction = hunger / HUNGER_ATTENTION_DIVISOR;
            return Math.Max(MIN_ATTENTION, BASE_ATTENTION - reduction);
        }
        
        /// <summary>
        /// Calculate weight carrying capacity based on health.
        /// Injured players have reduced carrying capacity.
        /// </summary>
        public int CalculateWeightLimit(int health)
        {
            // If health < 50, apply penalty to carrying capacity
            return health < HEALTH_THRESHOLD_FOR_CARRY ? INJURED_WEIGHT_PENALTY : 0;
        }
    }
}