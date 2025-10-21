using System;
using System.Collections.Generic;
using System.Linq;

public enum ObligationEffect
{
    // Entry Position Effects
    StatusPriority,         // Status letters enter at slot 3
    DiplomacyPriority,       // Diplomacy letters enter at slot 5
    TrustPriority,          // Trust letters enter at slot 7

    // Payment Bonuses
    DiplomacyBonus,         // Diplomacy letters +10 coins
    ShadowTriplePay,       // Shadow letters pay triple
    DiplomacyBonusPlus3,    // Diplomacy letters +3 coins bonus

    // Deadline Extensions
    TrustFreeExtend,       // Trust letters can extend deadline free
    DeadlinePlus2Days,     // Letters get +2 days to deadline

    // Forced DeliveryObligation Generation
    ShadowForced,          // Forced shadow letter every 3 days

    // Queue Action Restrictions
    NoStatusRefusal,       // Cannot refuse status letters
    NoDiplomacyPurge,       // Cannot purge diplomacy letters
    TrustSkipDoubleCost,   // Skipping trust letters costs double
    CannotRefuseLetters,   // Cannot refuse any letters

    // Leverage Modifiers
    ShadowEqualsStatus,    // Shadow letters use Status base position (3)
    MerchantRespect,       // Diplomacy letters with 5+ tokens get additional +1 position
    DebtSpiral,            // All negative token positions get additional -1

    // Dynamic Scaling Effects
    DynamicLeverageModifier,   // Leverage scales with token level
    DynamicPaymentBonus,       // Payment bonus scales with token level
    DynamicDeadlineBonus       // Deadline bonus scales with token level
}

public class StandingObligation
{
    // Identity
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = ""; // NPC or entity that granted this obligation

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // Mechanics
    public List<ObligationEffect> BenefitEffects { get; set; } = new List<ObligationEffect>();
    public List<ObligationEffect> ConstraintEffects { get; set; } = new List<ObligationEffect>();
    public ConnectionType? RelatedTokenType { get; set; } // Which token type this obligation affects

    // Threshold-based activation
    public string? RelatedNPCId { get; set; } // NPC whose tokens trigger this obligation
    public int? ActivationThreshold { get; set; } // Token threshold that activates this obligation
    public int? DeactivationThreshold { get; set; } // Token threshold that deactivates this obligation
    public bool IsThresholdBased { get; set; } = false; // Whether this obligation auto-activates
    public bool ActivatesAboveThreshold { get; set; } = true; // True = activates when above threshold, False = below

    // Status
    public int DayAccepted { get; set; } // Game day when obligation was accepted
    public bool IsActive { get; set; } = true;
    public int DaysSinceAccepted { get; set; } // Calculated during day transitions
    public bool WasAutoActivated { get; set; } = false; // Track if this was auto-activated by threshold

    // Tracking for forced generation effects
    public int DaysSinceLastForcedDeliveryObligation { get; set; } = 0;

    // Dynamic Scaling Properties
    public ScalingType ScalingType { get; set; } = ScalingType.None;
    public float ScalingFactor { get; set; } = 1.0f; // How much per token for linear scaling
    public float BaseValue { get; set; } = 0f; // Base value before scaling
    public float MinValue { get; set; } = 0f; // Minimum scaled value
    public float MaxValue { get; set; } = 100f; // Maximum scaled value

    // Stepped scaling thresholds (token count -> effect value)
    public Dictionary<int, float> SteppedThresholds { get; set; } = new Dictionary<int, float>();

    public StandingObligation()
    {
        // DayAccepted will be set when the obligation is activated in-game
    }

    // Check if this obligation affects a specific action
    public bool HasEffect(ObligationEffect effect)
    {
        return BenefitEffects.Contains(effect) || ConstraintEffects.Contains(effect);
    }

    // Check if this obligation applies to a specific token type
    public bool AppliesTo(ConnectionType tokenType)
    {
        return RelatedTokenType == null || RelatedTokenType == tokenType;
    }

    // Check if this obligation should be active based on current token count
    public bool ShouldBeActiveForTokenCount(int currentTokenCount)
    {
        if (!IsThresholdBased || !ActivationThreshold.HasValue)
            return IsActive; // Non-threshold based obligations maintain their current state

        if (ActivatesAboveThreshold)
        {
            // Activates when tokens are above threshold (e.g., Elena's Devotion at 5+)
            return currentTokenCount >= ActivationThreshold.Value;
        }
        else
        {
            // Activates when tokens are below threshold (e.g., Heavy Hand at -3 or worse)
            return currentTokenCount <= ActivationThreshold.Value;
        }
    }

    // Check if this obligation should deactivate based on current token count
    public bool ShouldDeactivateForTokenCount(int currentTokenCount)
    {
        if (!IsThresholdBased || !IsActive || !WasAutoActivated)
            return false; // Only auto-activated obligations can auto-deactivate

        // If no deactivation threshold is set, use activation threshold
        int deactivationPoint = DeactivationThreshold.HasValue ? DeactivationThreshold.Value : (ActivationThreshold.HasValue ? ActivationThreshold.Value : 0);

        if (ActivatesAboveThreshold)
        {
            // Deactivates when tokens drop below threshold
            return currentTokenCount < deactivationPoint;
        }
        else
        {
            // Deactivates when tokens rise above threshold
            return currentTokenCount > deactivationPoint;
        }
    }

    // Get summary of this obligation's effects
    public string GetEffectsSummary()
    {
        List<string> benefits = new List<string>();
        List<string> constraints = new List<string>();

        foreach (ObligationEffect effect in BenefitEffects)
        {
            benefits.Add(GetEffectDescription(effect));
        }

        foreach (ObligationEffect effect in ConstraintEffects)
        {
            constraints.Add(GetEffectDescription(effect));
        }

        string result = "";
        if (benefits.Any())
        {
            result += "Benefits: " + string.Join(", ", benefits);
        }
        if (constraints.Any())
        {
            if (result != "") result += "\n";
            result += "Constraints: " + string.Join(", ", constraints);
        }

        // Add threshold information if applicable
        if (IsThresholdBased && ActivationThreshold.HasValue)
        {
            if (result != "") result += "\n";
            string thresholdOperator = ActivatesAboveThreshold ? "≥" : "≤";
            result += $"Triggers at {RelatedTokenType} tokens {thresholdOperator} {ActivationThreshold}";
            if (!string.IsNullOrEmpty(RelatedNPCId))
            {
                result += $" with {RelatedNPCId}";
            }
        }

        return result;
    }

    private string GetEffectDescription(ObligationEffect effect)
    {
        return effect switch
        {
            ObligationEffect.StatusPriority => "Status letters enter at slot 3",
            ObligationEffect.DiplomacyPriority => "Diplomacy letters enter at slot 5",
            ObligationEffect.TrustPriority => "Trust letters enter at slot 7",
            ObligationEffect.DiplomacyBonus => "Diplomacy letters +10 coins",
            ObligationEffect.DiplomacyBonusPlus3 => "Diplomacy letters pay +3 coins bonus",
            ObligationEffect.ShadowTriplePay => "Shadow letters pay triple",
            ObligationEffect.TrustFreeExtend => "Trust letters extend deadline free",
            ObligationEffect.DeadlinePlus2Days => "Letters get +2 days to deadline",
            ObligationEffect.ShadowForced => "Forced shadow letter every 3 days",
            ObligationEffect.NoStatusRefusal => "Cannot refuse status letters",
            ObligationEffect.NoDiplomacyPurge => "Cannot purge diplomacy letters",
            ObligationEffect.TrustSkipDoubleCost => "Skipping trust letters costs double",
            ObligationEffect.CannotRefuseLetters => "Cannot refuse any letters",
            ObligationEffect.DynamicLeverageModifier => "Leverage scales with relationship",
            ObligationEffect.DynamicPaymentBonus => "Payment bonus scales with relationship",
            ObligationEffect.DynamicDeadlineBonus => "Deadline bonus scales with relationship",
            _ => effect.ToString()
        };
    }

    // Calculate dynamic value based on current token count
    public float CalculateDynamicValue(int tokenCount)
    {
        switch (ScalingType)
        {
            case ScalingType.None:
                return BaseValue;

            case ScalingType.Linear:
                // Linear scaling: base + (tokens * factor)
                float linearValue = BaseValue + (tokenCount * ScalingFactor);
                return Math.Max(MinValue, Math.Min(MaxValue, linearValue));

            case ScalingType.Stepped:
                // Find the highest threshold that we meet
                float steppedValue = BaseValue;
                foreach (KeyValuePair<int, float> threshold in SteppedThresholds.OrderBy(t => t.Key))
                {
                    if (tokenCount >= threshold.Key)
                    {
                        steppedValue = threshold.Value;
                    }
                }
                return steppedValue;

            case ScalingType.Threshold:
                // Only apply effect if we meet the threshold
                if (ActivationThreshold.HasValue)
                {
                    bool meetsThreshold = ActivatesAboveThreshold ?
                        tokenCount >= ActivationThreshold.Value :
                        tokenCount <= ActivationThreshold.Value;
                    return meetsThreshold ? BaseValue : 0f;
                }
                return BaseValue;

            default:
                return BaseValue;
        }
    }

}