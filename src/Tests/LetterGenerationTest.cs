using System;
using Xunit;

public class LetterGenerationTest
{
    [Fact]
    public void TestLetterTierCalculation()
    {
        // Test tier thresholds
        Assert.Equal(5, GetPaymentForFlow(7));   // Simple: 5-9 flow
        Assert.Equal(10, GetPaymentForFlow(12)); // Important: 10-14 flow
        Assert.Equal(15, GetPaymentForFlow(17)); // Urgent: 15-19 flow
        Assert.Equal(20, GetPaymentForFlow(25)); // Critical: 20+ flow
    }
    
    [Fact]
    public void TestDeadlineCalculation()
    {
        // Test deadline in segments
        Assert.Equal(36, GetDeadlineForFlow(7));  // Simple: 1 day
        Assert.Equal(18, GetDeadlineForFlow(12));  // Important: 12 seg
        Assert.Equal(9, GetDeadlineForFlow(17));  // Urgent: 9 seg
        Assert.Equal(3, GetDeadlineForFlow(25));  // Critical: 3 seg
    }
    
    private int GetPaymentForFlow(int flow)
    {
        if (flow >= 20) return 20;
        if (flow >= 15) return 15;
        if (flow >= 10) return 10;
        return 5;
    }
    
    private int GetDeadlineForFlow(int flow)
    {
        if (flow >= 20) return 3;  // 3 seg
        if (flow >= 15) return 9;  // 9 seg
        if (flow >= 10) return 18;  // 18 seg
        return 36; // 36 seg
    }
}