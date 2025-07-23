namespace Wayfarer.GameState.Operations;

/// <summary>
/// Operation to advance game time
/// </summary>
public class AdvanceTimeOperation : IGameOperation
{
    private readonly int _hours;
    private readonly TimeManager _timeManager;
    private int _previousHours;
    private int _previousDay;
    private TimeBlocks _previousTimeBlock;
    
    public AdvanceTimeOperation(int hours, TimeManager timeManager)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));
            
        _hours = hours;
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    }
    
    public string Description => $"Advance time by {_hours} hour(s)";
    
    public bool CanExecute(GameWorld gameWorld)
    {
        // Check if we have enough active hours remaining
        var activeHoursRemaining = _timeManager.GetActiveHoursRemaining();
        return activeHoursRemaining >= _hours;
    }
    
    public void Execute(GameWorld gameWorld)
    {
        // Store current state for rollback
        _previousHours = _timeManager.GetCurrentHours();
        _previousDay = gameWorld.CurrentDay;
        _previousTimeBlock = gameWorld.CurrentTimeBlock;
        
        // Advance time
        _timeManager.AdvanceTime(_hours);
    }
    
    public void Rollback(GameWorld gameWorld)
    {
        // Restore previous time state
        // Note: This is a simplified rollback - in a real implementation,
        // we might need to handle more complex state restoration
        gameWorld.CurrentDay = _previousDay;
        gameWorld.CurrentTimeBlock = _previousTimeBlock;
        
        // TimeManager doesn't expose a way to set hours directly,
        // so this would need to be enhanced in a real implementation
        // For now, we'll mark this as a limitation
        throw new NotImplementedException("Time rollback requires enhanced TimeManager API");
    }
}