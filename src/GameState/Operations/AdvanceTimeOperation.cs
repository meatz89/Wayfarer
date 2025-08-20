using System;

/// <summary>
/// Operation to advance game time
/// </summary>
public class AdvanceTimeOperation : IGameOperation
{
private readonly int _minutes;
private readonly TimeManager _timeManager;

public AdvanceTimeOperation(int minutes, TimeManager timeManager)
{
    if (minutes <= 0)
        throw new ArgumentException("Minutes must be positive", nameof(minutes));

    _minutes = minutes;
    _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
}

public string Description => $"Advance time by {_minutes} minute(s)";

public bool CanExecute(GameWorld gameWorld)
{
    return true;
}

public void Execute(GameWorld gameWorld)
{
    // Advance time
    _timeManager.AdvanceTime(_minutes);

    // CRITICAL: Update all letter deadlines when time advances
    // This creates the core tension - letters expire if not delivered
    Player? player = gameWorld.GetPlayer();

    // Update deadlines for letters in queue
    if (player?.ObligationQueue != null)
    {
        foreach (DeliveryObligation obligation in player.ObligationQueue)
        {
            if (obligation != null && !obligation.IsExpired)
            {
                obligation.DeadlineInMinutes -= _minutes;

                // Clamp to 0, never go negative
                if (obligation.DeadlineInMinutes < 0)
                {
                    obligation.DeadlineInMinutes = 0;
                }
            }
        }
    }

    // Note: CarriedLetters are physical Letter objects without deadlines
    // Only obligations in queue have deadlines that need updating
}
}