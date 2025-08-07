
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
        
        // CRITICAL: Update all letter deadlines when time advances
        // This creates the core tension - letters expire if not delivered
        var player = gameWorld.GetPlayer();
        
        // Update deadlines for letters in queue
        if (player?.LetterQueue != null)
        {
            foreach (var letter in player.LetterQueue)
            {
                if (letter != null && !letter.IsExpired)
                {
                    letter.DeadlineInHours -= _hours;
                    
                    // Clamp to 0, never go negative
                    if (letter.DeadlineInHours < 0)
                    {
                        letter.DeadlineInHours = 0;
                    }
                }
            }
        }
        
        // Also update deadlines for carried letters (physical letters in inventory)
        if (player?.CarriedLetters != null)
        {
            foreach (var letter in player.CarriedLetters)
            {
                if (letter != null && !letter.IsExpired)
                {
                    letter.DeadlineInHours -= _hours;
                    
                    // Clamp to 0, never go negative
                    if (letter.DeadlineInHours < 0)
                    {
                        letter.DeadlineInHours = 0;
                    }
                }
            }
        }
    }
}