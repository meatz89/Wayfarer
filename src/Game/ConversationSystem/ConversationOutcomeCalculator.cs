using System;
using Wayfarer.GameState;

namespace Wayfarer.Game.ConversationSystem
{
    /// <summary>
    /// Calculates Success/Neutral/Failure probabilities and determines actual outcomes for conversation choices.
    /// Based on the formula: Success Chance = (Patience - Difficulty + 5) × 12%
    /// </summary>
    public class ConversationOutcomeCalculator
    {
        /// <summary>
        /// Calculate probability distribution for a conversation choice
        /// </summary>
        public OutcomeProbabilities CalculateProbabilities(ConversationChoice choice, NPC npc, Player player, int currentPatience)
        {
            int difficulty = choice.PatienceCost; // Use patience cost as difficulty
            
            // Apply relationship modifiers to effective difficulty
            var tokens = player.NPCTokens.GetValueOrDefault(npc.ID, new Dictionary<ConnectionType, int>());
            int statusModifier = tokens.GetValueOrDefault(ConnectionType.Status, 0);
            int effectiveDifficulty = Math.Max(1, difficulty - statusModifier);
            
            // Base success chance formula from strategic-tactical-layer.md
            // REBALANCED: Reduced multiplier from 12 to 6 to create meaningful probability variations
            // Target: FREE=90%, 1-cost=84%, 2-cost=78%, 3-cost=72% at patience 10
            int calculation = (currentPatience - effectiveDifficulty + 5) * 6;
            int successChance = Math.Max(0, Math.Min(95, calculation));
            
            // DEBUG: Log the actual calculation values
            Console.WriteLine($"[OutcomeCalculator] Patience:{currentPatience}, Difficulty:{difficulty}, EffectiveDiff:{effectiveDifficulty}, Calc:{calculation}, Success:{successChance}%");
            
            // Distribute remaining probability between neutral and failure
            int remainingChance = 100 - successChance;
            int neutralChance = Math.Min(remainingChance, Math.Max(0, remainingChance * 2 / 3));
            int failureChance = remainingChance - neutralChance;
            
            return new OutcomeProbabilities
            {
                SuccessChance = successChance,
                NeutralChance = neutralChance,
                FailureChance = failureChance
            };
        }
        
        /// <summary>
        /// Determine actual outcome based on probabilities
        /// </summary>
        public ConversationOutcome DetermineOutcome(OutcomeProbabilities probabilities)
        {
            var random = new Random();
            int roll = random.Next(1, 101); // 1-100
            
            if (roll <= probabilities.SuccessChance)
            {
                return ConversationOutcome.Success;
            }
            else if (roll <= probabilities.SuccessChance + probabilities.NeutralChance)
            {
                return ConversationOutcome.Neutral;
            }
            else
            {
                return ConversationOutcome.Failure;
            }
        }
        
        /// <summary>
        /// Calculate the effects of a choice outcome
        /// </summary>
        public ConversationChoiceResult CalculateResult(ConversationChoice choice, ConversationOutcome outcome, NPC npc, Player player)
        {
            var result = new ConversationChoiceResult
            {
                Outcome = outcome,
                PatienceCost = GetEffectivePatienceCost(choice, player, npc),
                ComfortGain = 0,
                TokenChanges = new Dictionary<ConnectionType, int>(),
                NarrativeResult = ""
            };
            
            // Apply outcome-based effects
            switch (outcome)
            {
                case ConversationOutcome.Success:
                    result.ComfortGain = choice.ComfortGain;
                    result.NarrativeResult = GetSuccessNarrative(choice);
                    ApplySuccessEffects(result, choice, npc, player);
                    break;
                    
                case ConversationOutcome.Neutral:
                    result.ComfortGain = Math.Max(1, choice.ComfortGain / 2); // Partial comfort
                    result.NarrativeResult = GetNeutralNarrative(choice);
                    ApplyNeutralEffects(result, choice, npc, player);
                    break;
                    
                case ConversationOutcome.Failure:
                    result.ComfortGain = 0; // No comfort gain
                    result.NarrativeResult = GetFailureNarrative(choice);
                    ApplyFailureEffects(result, choice, npc, player);
                    break;
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculate effective patience cost with Commerce modifier
        /// </summary>
        private int GetEffectivePatienceCost(ConversationChoice choice, Player player, NPC npc)
        {
            int baseCost = choice.PatienceCost;
            
            // Commerce tokens reduce patience costs by 1 (minimum 1)
            var tokens = player.NPCTokens.GetValueOrDefault(npc.ID, new Dictionary<ConnectionType, int>());
            int commerceBonus = tokens.GetValueOrDefault(ConnectionType.Commerce, 0);
            
            return Math.Max(1, baseCost - commerceBonus);
        }
        
        private void ApplySuccessEffects(ConversationChoiceResult result, ConversationChoice choice, NPC npc, Player player)
        {
            // Apply mechanical effects from the choice card
            if (choice.MechanicalEffects != null)
            {
                foreach (var effect in choice.MechanicalEffects)
                {
                    // Apply full effect on success - effects handle their own application logic
                    // For now, just mark that effects should be applied - actual application happens in GameFacade
                }
            }
            
            // Success may provide bonus relationship gains
            if (choice.ComfortGain >= 3) // Significant choices build relationships
            {
                // Determine dominant relationship type for this choice
                var dominantType = GetDominantRelationshipType(choice);
                if (dominantType.HasValue)
                {
                    result.TokenChanges[dominantType.Value] = 1;
                }
            }
        }
        
        private void ApplyNeutralEffects(ConversationChoiceResult result, ConversationChoice choice, NPC npc, Player player)
        {
            // Apply reduced effects on neutral outcome
            if (choice.MechanicalEffects != null)
            {
                foreach (var effect in choice.MechanicalEffects)
                {
                    // Apply partial effect on neutral - handled in GameFacade
                    // For now, just mark that effects should be applied with reduced impact
                }
            }
        }
        
        private void ApplyFailureEffects(ConversationChoiceResult result, ConversationChoice choice, NPC npc, Player player)
        {
            // Failure may cause minor relationship damage for high-risk choices
            if (choice.PatienceCost >= 3) // Only expensive choices risk relationship damage
            {
                var dominantType = GetDominantRelationshipType(choice);
                if (dominantType.HasValue)
                {
                    result.TokenChanges[dominantType.Value] = -1;
                }
            }
        }
        
        private ConnectionType? GetDominantRelationshipType(ConversationChoice choice)
        {
            // Analyze choice description to determine relationship type
            // This is a simplified implementation - could be enhanced with more sophisticated analysis
            string description = choice.MechanicalDescription?.ToLower() ?? "";
            
            if (description.Contains("trust") || description.Contains("personal") || description.Contains("friend"))
                return ConnectionType.Trust;
            if (description.Contains("commerce") || description.Contains("business") || description.Contains("coin"))
                return ConnectionType.Commerce;
            if (description.Contains("status") || description.Contains("noble") || description.Contains("respect"))
                return ConnectionType.Status;
            if (description.Contains("shadow") || description.Contains("secret") || description.Contains("hidden"))
                return ConnectionType.Shadow;
                
            return null; // No clear dominant type
        }
        
        private string GetSuccessNarrative(ConversationChoice choice)
        {
            return "Your words resonate perfectly. They nod in understanding.";
        }
        
        private string GetNeutralNarrative(ConversationChoice choice)
        {
            return "Your approach is adequate. They seem somewhat convinced.";
        }
        
        private string GetFailureNarrative(ConversationChoice choice)
        {
            return "Your words miss the mark. They seem unconvinced by your approach.";
        }
    }
    
    /// <summary>
    /// Probability distribution for a conversation choice outcome
    /// </summary>
    public class OutcomeProbabilities
    {
        public int SuccessChance { get; set; }
        public int NeutralChance { get; set; }
        public int FailureChance { get; set; }
    }
    
    /// <summary>
    /// Possible outcomes for a conversation choice
    /// </summary>
    public enum ConversationOutcome
    {
        Success,
        Neutral, 
        Failure
    }
    
    /// <summary>
    /// Complete result of a conversation choice including effects
    /// </summary>
    public class ConversationChoiceResult
    {
        public ConversationOutcome Outcome { get; set; }
        public int PatienceCost { get; set; }
        public int ComfortGain { get; set; }
        public Dictionary<ConnectionType, int> TokenChanges { get; set; } = new();
        public string NarrativeResult { get; set; } = "";
    }
}