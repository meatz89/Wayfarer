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
        
        /// <summary>
        /// Calculate conversation patience based on hunger and emotional state.
        /// </summary>
        public int CalculatePatience(int hunger, EmotionalState emotionalState)
        {
            int basePatience = emotionalState switch
            {
                EmotionalState.HOSTILE => 3,
                EmotionalState.GUARDED => 5,
                EmotionalState.NEUTRAL => 8,
                EmotionalState.OPEN => 10,
                EmotionalState.EAGER => 12,
                EmotionalState.CONNECTED => 15,
                _ => 8
            };
            
            // Reduce patience if very hungry
            if (hunger >= 80) basePatience -= 3;
            else if (hunger >= 60) basePatience -= 2;
            else if (hunger >= 40) basePatience -= 1;
            
            return Math.Max(1, basePatience);
        }
        
        /// <summary>
        /// Calculate health regeneration rate based on hunger.
        /// </summary>
        public int CalculateHealthRegen(int hunger)
        {
            if (hunger >= 80) return 0; // No regen when starving
            if (hunger >= 50) return 1; // Slow regen when hungry
            if (hunger >= 30) return 2; // Normal regen
            return 3; // Fast regen when well-fed
        }
        
        /// <summary>
        /// Calculate stamina cost multiplier based on health.
        /// </summary>
        public double CalculateStaminaCostMultiplier(int health)
        {
            if (health < 30) return 2.0; // Double cost when badly injured
            if (health < 50) return 1.5; // 50% more cost when injured
            if (health < 70) return 1.2; // 20% more cost when hurt
            return 1.0; // Normal cost when healthy
        }
        
        /// <summary>
        /// Calculate coin value with merchant bonuses.
        /// </summary>
        public int CalculateMerchantPrice(int basePrice, int commerceTokens)
        {
            // Each commerce token gives 5% discount, max 50%
            double discount = Math.Min(0.5, commerceTokens * 0.05);
            return (int)(basePrice * (1.0 - discount));
        }
    }
}