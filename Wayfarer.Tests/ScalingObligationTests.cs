using Xunit;
using System.Collections.Generic;

public class ScalingObligationTests
{
    [Fact]
    public void LinearScaling_CalculatesCorrectly()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "test_linear",
            Name = "Test Linear Scaling",
            ScalingType = ScalingType.Linear,
            ScalingFactor = 0.5f,
            BaseValue = 2f,
            MinValue = 0f,
            MaxValue = 10f
        };

        // Act & Assert
        Assert.Equal(2f, obligation.CalculateDynamicValue(0));      // Base value
        Assert.Equal(4.5f, obligation.CalculateDynamicValue(5));    // 2 + (5 * 0.5)
        Assert.Equal(7f, obligation.CalculateDynamicValue(10));     // 2 + (10 * 0.5)
        Assert.Equal(10f, obligation.CalculateDynamicValue(20));    // Clamped to max
        Assert.Equal(0f, obligation.CalculateDynamicValue(-10));    // Clamped to min
    }

    [Fact]
    public void SteppedScaling_UsesCorrectThresholds()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "test_stepped",
            Name = "Test Stepped Scaling",
            ScalingType = ScalingType.Stepped,
            BaseValue = 0f,
            SteppedThresholds = new Dictionary<int, float>
            {
                { 3, 2f },
                { 5, 5f },
                { 8, 10f },
                { 10, 15f }
            }
        };

        // Act & Assert
        Assert.Equal(0f, obligation.CalculateDynamicValue(0));      // Below first threshold
        Assert.Equal(0f, obligation.CalculateDynamicValue(2));      // Still below
        Assert.Equal(2f, obligation.CalculateDynamicValue(3));      // At first threshold
        Assert.Equal(2f, obligation.CalculateDynamicValue(4));      // Between thresholds
        Assert.Equal(5f, obligation.CalculateDynamicValue(5));      // At second threshold
        Assert.Equal(10f, obligation.CalculateDynamicValue(9));     // Above third threshold
        Assert.Equal(15f, obligation.CalculateDynamicValue(15));    // Above all thresholds
    }

    [Fact]
    public void ThresholdScaling_AppliesTotalValueOrZero()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "test_threshold",
            Name = "Test Threshold Scaling",
            ScalingType = ScalingType.Threshold,
            BaseValue = 5f,
            ActivationThreshold = 4,
            ActivatesAboveThreshold = true
        };

        // Act & Assert
        Assert.Equal(0f, obligation.CalculateDynamicValue(3));      // Below threshold
        Assert.Equal(5f, obligation.CalculateDynamicValue(4));      // At threshold
        Assert.Equal(5f, obligation.CalculateDynamicValue(10));     // Above threshold
    }

    [Fact]
    public void DynamicLeverageModifier_ScalesPosition()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "patron_leverage",
            Name = "Patron Leverage",
            BenefitEffects = new List<ObligationEffect> { ObligationEffect.DynamicLeverageModifier },
            RelatedTokenType = ConnectionType.Noble,
            ScalingType = ScalingType.Linear,
            ScalingFactor = -0.5f,  // Negative factor: negative tokens produce positive modifier
            BaseValue = 0f,
            MinValue = 0f,
            MaxValue = 3f
        };

        var letter = new Letter
        {
            TokenType = ConnectionType.Noble,
            SenderName = "Patron"
        };

        // Act & Assert
        Assert.Equal(5, obligation.CalculateDynamicLeverage(letter, 5, 0));      // No change at 0 tokens
        Assert.Equal(4, obligation.CalculateDynamicLeverage(letter, 5, -2));     // -1 leverage per -2 debt tokens
        Assert.Equal(2, obligation.CalculateDynamicLeverage(letter, 5, -6));     // Max leverage of -3
        Assert.Equal(1, obligation.CalculateDynamicLeverage(letter, 3, -6));     // Clamps to position 1
    }

    [Fact]
    public void DynamicPaymentBonus_ScalesPayment()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "merchant_bonus",
            Name = "Merchant Bonus",
            BenefitEffects = new List<ObligationEffect> { ObligationEffect.DynamicPaymentBonus },
            RelatedTokenType = ConnectionType.Trade,
            ScalingType = ScalingType.Stepped,
            BaseValue = 0f,
            SteppedThresholds = new Dictionary<int, float>
            {
                { 3, 2f },
                { 5, 5f },
                { 8, 10f }
            }
        };

        var letter = new Letter
        {
            TokenType = ConnectionType.Trade,
            Payment = 10
        };

        // Act & Assert
        Assert.Equal(0, obligation.CalculateDynamicPaymentBonus(letter, 10, 2));   // Below threshold
        Assert.Equal(2, obligation.CalculateDynamicPaymentBonus(letter, 10, 3));   // +2 coins at 3 tokens
        Assert.Equal(5, obligation.CalculateDynamicPaymentBonus(letter, 10, 6));   // +5 coins at 6 tokens
        Assert.Equal(10, obligation.CalculateDynamicPaymentBonus(letter, 10, 10)); // +10 coins at 10 tokens
    }

    [Fact]
    public void DynamicDeadlineBonus_ScalesDeadline()
    {
        // Arrange
        var obligation = new StandingObligation
        {
            ID = "trust_flexibility",
            Name = "Trust Flexibility",
            BenefitEffects = new List<ObligationEffect> { ObligationEffect.DynamicDeadlineBonus },
            RelatedTokenType = ConnectionType.Trust,
            ScalingType = ScalingType.Linear,
            ScalingFactor = 0.5f,
            BaseValue = 0f,
            MinValue = 0f,
            MaxValue = 4f
        };

        var letter = new Letter
        {
            TokenType = ConnectionType.Trust,
            Deadline = 3
        };

        // Act & Assert
        Assert.Equal(0, obligation.CalculateDynamicDeadlineBonus(letter, 0));   // No bonus at 0 tokens
        Assert.Equal(1, obligation.CalculateDynamicDeadlineBonus(letter, 2));   // +1 day at 2 tokens
        Assert.Equal(2, obligation.CalculateDynamicDeadlineBonus(letter, 5));   // +2.5 days rounds to 2
        Assert.Equal(4, obligation.CalculateDynamicDeadlineBonus(letter, 10));  // Max +4 days
    }
}