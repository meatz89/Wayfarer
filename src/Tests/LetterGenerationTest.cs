using System;
using Xunit;

public class LetterGenerationTest
{
    [Fact]
    public void TestLetterTierCalculation()
    {
        // Test tier thresholds
        Assert.Equal(5, GetPaymentForComfort(7));   // Simple: 5-9 comfort
        Assert.Equal(10, GetPaymentForComfort(12)); // Important: 10-14 comfort
        Assert.Equal(15, GetPaymentForComfort(17)); // Urgent: 15-19 comfort
        Assert.Equal(20, GetPaymentForComfort(25)); // Critical: 20+ comfort
    }
    
    [Fact]
    public void TestDeadlineCalculation()
    {
        // Test deadline in minutes
        Assert.Equal(1440, GetDeadlineForComfort(7));  // Simple: 24h
        Assert.Equal(720, GetDeadlineForComfort(12));  // Important: 12h
        Assert.Equal(360, GetDeadlineForComfort(17));  // Urgent: 6h
        Assert.Equal(120, GetDeadlineForComfort(25));  // Critical: 2h
    }
    
    private int GetPaymentForComfort(int comfort)
    {
        if (comfort >= 20) return 20;
        if (comfort >= 15) return 15;
        if (comfort >= 10) return 10;
        return 5;
    }
    
    private int GetDeadlineForComfort(int comfort)
    {
        if (comfort >= 20) return 120;  // 2h
        if (comfort >= 15) return 360;  // 6h
        if (comfort >= 10) return 720;  // 12h
        return 1440; // 24h
    }
}