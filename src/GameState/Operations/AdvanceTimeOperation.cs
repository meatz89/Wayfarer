
/// <summary>
/// Operation to advance game time
/// </summary>
public class AdvanceTimeOperation : IGameOperation
{
    private readonly int _hours;
    private readonly TimeManager _timeManager;

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
        return true;
    }

    public void Execute(GameWorld gameWorld)
    {
        // Advance time
        _timeManager.AdvanceTime(_hours);
    }
}