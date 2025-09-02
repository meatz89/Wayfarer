using System;
using Xunit;

public class ComfortBatteryTests
{
    [Fact]
    public void TestInitialState()
    {
        // Battery always starts at 0
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        Assert.Equal(0, battery.CurrentComfort);
        Assert.Equal(EmotionalState.NEUTRAL, battery.CurrentState);
    }

    [Fact]
    public void TestComfortRange()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Apply large positive change - should clamp at 3
        var result = battery.ApplyComfortChange(10);
        Assert.Equal(3, battery.CurrentComfort);
        
        // Transition should occur at exactly 3
        Assert.True(result.stateChanged);
        Assert.Equal(EmotionalState.OPEN, result.newState);
    }

    [Fact]
    public void TestPositiveTransition()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Apply +2 comfort
        var result = battery.ApplyComfortChange(2);
        Assert.False(result.stateChanged);
        Assert.Equal(2, battery.CurrentComfort);
        Assert.Equal(EmotionalState.NEUTRAL, battery.CurrentState);
        
        // Apply +1 more - should trigger transition at exactly 3
        result = battery.ApplyComfortChange(1);
        Assert.True(result.stateChanged);
        Assert.Equal(EmotionalState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentComfort); // Reset to 0!
    }

    [Fact]
    public void TestNegativeTransition()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Apply -2 comfort
        var result = battery.ApplyComfortChange(-2);
        Assert.False(result.stateChanged);
        Assert.Equal(-2, battery.CurrentComfort);
        
        // Apply -1 more - should trigger transition at exactly -3
        result = battery.ApplyComfortChange(-1);
        Assert.True(result.stateChanged);
        Assert.Equal(EmotionalState.TENSE, result.newState);
        Assert.Equal(0, battery.CurrentComfort); // Reset to 0!
    }

    [Fact]
    public void TestDesperateEndsAtNegativeThree()
    {
        var battery = new ComfortBatteryManager(EmotionalState.DESPERATE);
        
        // Apply -3 comfort directly
        var result = battery.ApplyComfortChange(-3);
        Assert.True(result.conversationEnds);
        Assert.Equal(EmotionalState.DESPERATE, result.newState);
        Assert.Equal(-3, battery.CurrentComfort);
    }

    [Fact]
    public void TestDesperateCanRecover()
    {
        var battery = new ComfortBatteryManager(EmotionalState.DESPERATE);
        
        // Apply +3 comfort to transition up
        var result = battery.ApplyComfortChange(3);
        Assert.True(result.stateChanged);
        Assert.Equal(EmotionalState.TENSE, result.newState);
        Assert.Equal(0, battery.CurrentComfort); // Reset after transition
        Assert.False(result.conversationEnds);
    }

    [Fact]
    public void TestConnectedCantGoHigher()
    {
        var battery = new ComfortBatteryManager(EmotionalState.CONNECTED);
        
        // Apply +3 comfort
        var result = battery.ApplyComfortChange(3);
        Assert.False(result.stateChanged); // Already at max
        Assert.Equal(EmotionalState.CONNECTED, result.newState);
        Assert.Equal(3, battery.CurrentComfort); // Stays at 3
    }

    [Fact]
    public void TestVolatileAtmosphere()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // +2 becomes +3 with Volatile
        var result = battery.ApplyComfortChange(2, AtmosphereType.Volatile);
        Assert.True(result.stateChanged); // Should transition
        Assert.Equal(EmotionalState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentComfort); // Reset after transition
    }

    [Fact]
    public void TestExposedAtmosphere()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // +2 becomes +4 with Exposed, clamped to +3
        var result = battery.ApplyComfortChange(2, AtmosphereType.Exposed);
        Assert.True(result.stateChanged); // Should transition at 3
        Assert.Equal(EmotionalState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentComfort); // Reset after transition
    }

    [Fact]
    public void TestStateProgression()
    {
        var battery = new ComfortBatteryManager(EmotionalState.DESPERATE);
        
        // Progress through all states upward
        var result = battery.ApplyComfortChange(3);
        Assert.Equal(EmotionalState.TENSE, result.newState);
        
        result = battery.ApplyComfortChange(3);
        Assert.Equal(EmotionalState.NEUTRAL, result.newState);
        
        result = battery.ApplyComfortChange(3);
        Assert.Equal(EmotionalState.OPEN, result.newState);
        
        result = battery.ApplyComfortChange(3);
        Assert.Equal(EmotionalState.CONNECTED, result.newState);
        
        // Can't go higher
        result = battery.ApplyComfortChange(3);
        Assert.Equal(EmotionalState.CONNECTED, result.newState);
    }

    [Fact]
    public void TestResetToZero()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Set comfort to 2
        battery.ApplyComfortChange(2);
        Assert.Equal(2, battery.CurrentComfort);
        
        // Reset to zero (observation card effect)
        battery.ResetToZero();
        Assert.Equal(0, battery.CurrentComfort);
    }

    [Fact]
    public void TestTransitionWarnings()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // At +2, should warn about positive transition
        battery.ApplyComfortChange(2);
        var warning = battery.GetTransitionWarning();
        Assert.Contains("One more positive", warning);
        Assert.Contains("OPEN", warning);
        
        // Reset and test negative
        battery.ResetToZero();
        battery.ApplyComfortChange(-2);
        warning = battery.GetTransitionWarning();
        Assert.Contains("One more negative", warning);
        Assert.Contains("TENSE", warning);
    }

    [Fact]
    public void TestDesperateWarning()
    {
        var battery = new ComfortBatteryManager(EmotionalState.DESPERATE);
        
        // At -2 in DESPERATE state, should warn about conversation ending
        battery.ApplyComfortChange(-2);
        var warning = battery.GetTransitionWarning();
        Assert.Contains("WARNING", warning);
        Assert.Contains("end conversation", warning);
    }

    [Fact]
    public void TestComfortDisplay()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Test display at 0
        var display = battery.GetCompactDisplay();
        Assert.Equal("0", display);
        
        // Test positive display
        battery.ApplyComfortChange(2);
        display = battery.GetCompactDisplay();
        Assert.Equal("+2", display);
        
        // Test negative display
        battery.ResetToZero();
        battery.ApplyComfortChange(-2);
        display = battery.GetCompactDisplay();
        Assert.Equal("-2", display);
    }
    
    [Fact]
    public void TestNoComfortBankingBeyondThree()
    {
        var battery = new ComfortBatteryManager(EmotionalState.NEUTRAL);
        
        // Apply +2
        battery.ApplyComfortChange(2);
        Assert.Equal(2, battery.CurrentComfort);
        
        // Apply +3 more - should only go to 3 and trigger transition
        var result = battery.ApplyComfortChange(3);
        Assert.True(result.stateChanged);
        Assert.Equal(0, battery.CurrentComfort); // Reset, not 5
        Assert.Equal(EmotionalState.OPEN, result.newState);
    }
}