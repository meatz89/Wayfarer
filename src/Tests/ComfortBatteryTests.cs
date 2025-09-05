using System;
using Xunit;

public class FlowBatteryTests
{
    [Fact]
    public void TestInitialState()
    {
        // Battery always starts at 0
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        Assert.Equal(0, battery.CurrentFlow);
        Assert.Equal(ConnectionState.NEUTRAL, battery.CurrentState);
    }

    [Fact]
    public void TestFlowRange()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Apply large positive change - should clamp at 3
        var result = battery.ApplyFlowChange(10);
        Assert.Equal(3, battery.CurrentFlow);
        
        // Transition should occur at exactly 3
        Assert.True(result.stateChanged);
        Assert.Equal(ConnectionState.OPEN, result.newState);
    }

    [Fact]
    public void TestPositiveTransition()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Apply +2 flow
        var result = battery.ApplyFlowChange(2);
        Assert.False(result.stateChanged);
        Assert.Equal(2, battery.CurrentFlow);
        Assert.Equal(ConnectionState.NEUTRAL, battery.CurrentState);
        
        // Apply +1 more - should trigger transition at exactly 3
        result = battery.ApplyFlowChange(1);
        Assert.True(result.stateChanged);
        Assert.Equal(ConnectionState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentFlow); // Reset to 0!
    }

    [Fact]
    public void TestNegativeTransition()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Apply -2 flow
        var result = battery.ApplyFlowChange(-2);
        Assert.False(result.stateChanged);
        Assert.Equal(-2, battery.CurrentFlow);
        
        // Apply -1 more - should trigger transition at exactly -3
        result = battery.ApplyFlowChange(-1);
        Assert.True(result.stateChanged);
        Assert.Equal(ConnectionState.GUARDED, result.newState);
        Assert.Equal(0, battery.CurrentFlow); // Reset to 0!
    }

    [Fact]
    public void TestDisconnectedEndsAtNegativeThree()
    {
        var battery = new FlowBatteryManager(ConnectionState.DISCONNECTED);
        
        // Apply -3 flow directly
        var result = battery.ApplyFlowChange(-3);
        Assert.True(result.conversationEnds);
        Assert.Equal(ConnectionState.DISCONNECTED, result.newState);
        Assert.Equal(-3, battery.CurrentFlow);
    }

    [Fact]
    public void TestDisconnectedCanRecover()
    {
        var battery = new FlowBatteryManager(ConnectionState.DISCONNECTED);
        
        // Apply +3 flow to transition up
        var result = battery.ApplyFlowChange(3);
        Assert.True(result.stateChanged);
        Assert.Equal(ConnectionState.GUARDED, result.newState);
        Assert.Equal(0, battery.CurrentFlow); // Reset after transition
        Assert.False(result.conversationEnds);
    }

    [Fact]
    public void TestConnectedCantGoHigher()
    {
        var battery = new FlowBatteryManager(ConnectionState.CONNECTED);
        
        // Apply +3 flow
        var result = battery.ApplyFlowChange(3);
        Assert.False(result.stateChanged); // Already at max
        Assert.Equal(ConnectionState.CONNECTED, result.newState);
        Assert.Equal(3, battery.CurrentFlow); // Stays at 3
    }

    [Fact]
    public void TestVolatileAtmosphere()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // +2 becomes +3 with Volatile
        var result = battery.ApplyFlowChange(2, AtmosphereType.Volatile);
        Assert.True(result.stateChanged); // Should transition
        Assert.Equal(ConnectionState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentFlow); // Reset after transition
    }

    [Fact]
    public void TestExposedAtmosphere()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // +2 becomes +4 with Exposed, clamped to +3
        var result = battery.ApplyFlowChange(2, AtmosphereType.Exposed);
        Assert.True(result.stateChanged); // Should transition at 3
        Assert.Equal(ConnectionState.OPEN, result.newState);
        Assert.Equal(0, battery.CurrentFlow); // Reset after transition
    }

    [Fact]
    public void TestStateProgression()
    {
        var battery = new FlowBatteryManager(ConnectionState.DISCONNECTED);
        
        // Progress through all states upward
        var result = battery.ApplyFlowChange(3);
        Assert.Equal(ConnectionState.GUARDED, result.newState);
        
        result = battery.ApplyFlowChange(3);
        Assert.Equal(ConnectionState.NEUTRAL, result.newState);
        
        result = battery.ApplyFlowChange(3);
        Assert.Equal(ConnectionState.OPEN, result.newState);
        
        result = battery.ApplyFlowChange(3);
        Assert.Equal(ConnectionState.CONNECTED, result.newState);
        
        // Can't go higher
        result = battery.ApplyFlowChange(3);
        Assert.Equal(ConnectionState.CONNECTED, result.newState);
    }

    [Fact]
    public void TestResetToZero()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Set flow to 2
        battery.ApplyFlowChange(2);
        Assert.Equal(2, battery.CurrentFlow);
        
        // Reset to zero (observation card effect)
        battery.ResetToZero();
        Assert.Equal(0, battery.CurrentFlow);
    }

    [Fact]
    public void TestTransitionWarnings()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // At +2, should warn about positive transition
        battery.ApplyFlowChange(2);
        var warning = battery.GetTransitionWarning();
        Assert.Contains("One more positive", warning);
        Assert.Contains(RECEPTIVE, warning);
        
        // Reset and test negative
        battery.ResetToZero();
        battery.ApplyFlowChange(-2);
        warning = battery.GetTransitionWarning();
        Assert.Contains("One more negative", warning);
        Assert.Contains("GUARDED", warning);
    }

    [Fact]
    public void TestDisconnectedWarning()
    {
        var battery = new FlowBatteryManager(ConnectionState.DISCONNECTED);
        
        // At -2 in DISCONNECTED state, should warn about conversation ending
        battery.ApplyFlowChange(-2);
        var warning = battery.GetTransitionWarning();
        Assert.Contains("WARNING", warning);
        Assert.Contains("end conversation", warning);
    }

    [Fact]
    public void TestFlowDisplay()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Test display at 0
        var display = battery.GetCompactDisplay();
        Assert.Equal("0", display);
        
        // Test positive display
        battery.ApplyFlowChange(2);
        display = battery.GetCompactDisplay();
        Assert.Equal("+2", display);
        
        // Test negative display
        battery.ResetToZero();
        battery.ApplyFlowChange(-2);
        display = battery.GetCompactDisplay();
        Assert.Equal("-2", display);
    }
    
    [Fact]
    public void TestNoFlowBankingBeyondThree()
    {
        var battery = new FlowBatteryManager(ConnectionState.NEUTRAL);
        
        // Apply +2
        battery.ApplyFlowChange(2);
        Assert.Equal(2, battery.CurrentFlow);
        
        // Apply +3 more - should only go to 3 and trigger transition
        var result = battery.ApplyFlowChange(3);
        Assert.True(result.stateChanged);
        Assert.Equal(0, battery.CurrentFlow); // Reset, not 5
        Assert.Equal(ConnectionState.OPEN, result.newState);
    }
}