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
        // Test deadline in minutes
        Assert.Equal(1440, GetDeadlineForFlow(7));  // Simple: 24h
        Assert.Equal(720, GetDeadlineForFlow(12));  // Important: 12h
        Assert.Equal(360, GetDeadlineForFlow(17));  // Urgent: 6h
        Assert.Equal(120, GetDeadlineForFlow(25));  // Critical: 2h
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
        if (flow >= 20) return 120;  // 2h
        if (flow >= 15) return 360;  // 6h
        if (flow >= 10) return 720;  // 12h
        return 1440; // 24h
    }
}