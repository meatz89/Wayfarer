using System;
using System.Collections.Generic;
using System.Linq;

public enum ObligationEffect
{
    // Entry Position Effects
    NoblesPriority,        // Noble letters enter at slot 5
    CommonFolksPriority,   // Common letters enter at slot 6
    PatronJumpToTop,       // Patron letters jump to slots 1-3

    // Payment Bonuses
    TradeBonus,            // Trade letters +10 coins
    ShadowTriplePay,       // Shadow letters pay triple

    // Deadline Extensions
    TrustFreeExtend,       // Trust letters can extend deadline free

    // Forced Letter Generation
    ShadowForced,          // Forced shadow letter every 3 days
    PatronMonthly,         // Monthly patron resource package

    // Queue Action Restrictions
    NoNobleRefusal,        // Cannot refuse noble letters
    NoTradePurge,          // Cannot purge trade letters
    TrustSkipDoubleCost,   // Skipping trust letters costs double
    NoCommonRefusal,       // Refusing common letters loses 2 tokens

    // Leverage Modifiers
    ShadowEqualsNoble,     // Shadow letters use Noble base position (3)
    MerchantRespect,       // Trade letters with 5+ tokens get additional +1 position
    CommonRevenge,         // Common letters from debt relationships use position 3
    PatronAbsolute,        // Patron letters push everything down (no displacement limit)
    DebtSpiral             // All negative token positions get additional -1
}

public class StandingObligation
{
    // Identity
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Source { get; set; } = ""; // NPC or entity that granted this obligation

    // Mechanics
    public List<ObligationEffect> BenefitEffects { get; set; } = new List<ObligationEffect>();
    public List<ObligationEffect> ConstraintEffects { get; set; } = new List<ObligationEffect>();
    public ConnectionType? RelatedTokenType { get; set; } // Which token type this obligation affects

    // Status
    public int DayAccepted { get; set; } // Game day when obligation was accepted
    public bool IsActive { get; set; } = true;
    public int DaysSinceAccepted { get; set; } // Calculated during day transitions

    // Tracking for forced generation effects
    public int DaysSinceLastForcedLetter { get; set; } = 0;

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

    // Calculate coin bonus for letter delivery
    public int CalculateCoinBonus(Letter letter, int basePayment)
    {
        if (!AppliesTo(letter.TokenType)) return 0;

        if (HasEffect(ObligationEffect.TradeBonus) && letter.TokenType == ConnectionType.Trade)
        {
            return 10; // Flat +10 bonus
        }

        if (HasEffect(ObligationEffect.ShadowTriplePay) && letter.TokenType == ConnectionType.Shadow)
        {
            return basePayment * 2; // Triple = base + (2 * base)
        }

        return 0;
    }

    // Determine entry position for new letters
    public int CalculateEntryPosition(Letter letter, int defaultPosition)
    {
        if (!AppliesTo(letter.TokenType)) return defaultPosition;

        if (HasEffect(ObligationEffect.NoblesPriority) && letter.TokenType == ConnectionType.Noble)
        {
            return Math.Min(5, defaultPosition); // Enter at slot 5 or higher
        }

        if (HasEffect(ObligationEffect.CommonFolksPriority) && letter.TokenType == ConnectionType.Common)
        {
            return Math.Min(6, defaultPosition); // Enter at slot 6 or higher
        }

        if (HasEffect(ObligationEffect.PatronJumpToTop) && letter.IsFromPatron)
        {
            return 1; // Jump to position 1-3 (will be handled by queue manager)
        }

        return defaultPosition;
    }

    // Check if deadline extension is free
    public bool IsFreeDeadlineExtension(Letter letter)
    {
        return HasEffect(ObligationEffect.TrustFreeExtend) &&
               letter.TokenType == ConnectionType.Trust;
    }

    // Calculate skip cost multiplier
    public int CalculateSkipCostMultiplier(Letter letter)
    {
        if (HasEffect(ObligationEffect.TrustSkipDoubleCost) &&
            letter.TokenType == ConnectionType.Trust)
        {
            return 2; // Double cost
        }

        return 1; // Normal cost
    }

    // Check if action is forbidden by constraints
    public bool IsForbiddenAction(string actionType, Letter letter)
    {
        if (actionType == "refuse" && HasEffect(ObligationEffect.NoNobleRefusal) &&
            letter.TokenType == ConnectionType.Noble)
        {
            return true;
        }

        if (actionType == "refuse" && HasEffect(ObligationEffect.NoCommonRefusal) &&
            letter.TokenType == ConnectionType.Common)
        {
            return true;
        }

        if (actionType == "purge" && HasEffect(ObligationEffect.NoTradePurge) &&
            letter.TokenType == ConnectionType.Trade)
        {
            return true;
        }

        return false;
    }

    // Check if forced letter should be generated
    public bool ShouldGenerateForcedLetter()
    {
        if (HasEffect(ObligationEffect.ShadowForced))
        {
            return DaysSinceLastForcedLetter >= 3; // Every 3 days
        }

        if (HasEffect(ObligationEffect.PatronMonthly))
        {
            return DaysSinceLastForcedLetter >= 30; // Monthly
        }

        return false;
    }

    // Record forced letter generation
    public void RecordForcedLetterGenerated()
    {
        DaysSinceLastForcedLetter = 0;
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

        return result;
    }

    private string GetEffectDescription(ObligationEffect effect)
    {
        return effect switch
        {
            ObligationEffect.NoblesPriority => "Noble letters enter at slot 5",
            ObligationEffect.CommonFolksPriority => "Common letters enter at slot 6",
            ObligationEffect.PatronJumpToTop => "Patron letters jump to top slots",
            ObligationEffect.TradeBonus => "Trade letters +10 coins",
            ObligationEffect.ShadowTriplePay => "Shadow letters pay triple",
            ObligationEffect.TrustFreeExtend => "Trust letters extend deadline free",
            ObligationEffect.ShadowForced => "Forced shadow letter every 3 days",
            ObligationEffect.PatronMonthly => "Monthly patron resource package",
            ObligationEffect.NoNobleRefusal => "Cannot refuse noble letters",
            ObligationEffect.NoTradePurge => "Cannot purge trade letters",
            ObligationEffect.TrustSkipDoubleCost => "Skipping trust letters costs double",
            ObligationEffect.NoCommonRefusal => "Refusing common letters loses 2 tokens",
            _ => effect.ToString()
        };
    }
}