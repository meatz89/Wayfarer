using System;

/// <summary>
/// Operation to advance game time
/// </summary>
public class AdvanceTimeOperation : IGameOperation
{
    private readonly int _segments;
    private readonly TimeManager _timeManager;

    public AdvanceTimeOperation(int segments, TimeManager timeManager)
    {
        if (segments <= 0)
            throw new ArgumentException("Segments must be positive", nameof(segments));

        _segments = segments;
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    }

    public string Description => $"Advance time by {_segments} segment(s)";

    public bool CanExecute(GameWorld gameWorld)
    {
        return true;
    }

    public void Execute(GameWorld gameWorld)
    {
        // Advance time using segment-based system
        _timeManager.AdvanceSegments(_segments);

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
                    obligation.DeadlineInSegments -= _segments;

                    // Clamp to 0, never go negative
                    if (obligation.DeadlineInSegments < 0)
                    {
                        obligation.DeadlineInSegments = 0;
                    }
                }
            }
        }

        // Note: CarriedLetters are physical Letter objects without deadlines
        // Only obligations in queue have deadlines that need updating
    }
}