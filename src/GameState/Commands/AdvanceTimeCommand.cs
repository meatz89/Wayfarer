using System;
using System.Threading.Tasks;


/// <summary>
/// Command to advance game time by a specified number of hours.
/// </summary>
public class AdvanceTimeCommand : BaseGameCommand
{
    private readonly int _hours;
    private readonly string _reason;
    private readonly bool _forceAdvance;

    public AdvanceTimeCommand(int hours, string reason = "activity", bool forceAdvance = false)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));

        _hours = hours;
        _reason = reason;
        _forceAdvance = forceAdvance;

        Description = $"Advance time by {_hours} hour(s) for {_reason}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Check if we can spend active hours (unless forced, like sleeping)
        if (!_forceAdvance && !gameWorld.TimeManager.CanPerformAction(_hours))
        {
            int remaining = gameWorld.TimeManager.HoursRemaining;
            return CommandValidationResult.Failure(
                $"Not enough active hours remaining. Need {_hours}, have {remaining}",
                canBeRemedied: true,
                remediationHint: "Rest for the night to start a new day"
            );
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        CommandValidationResult validation = CanExecute(gameWorld);
        if (!validation.IsValid && !_forceAdvance)
        {
            return CommandResult.Failure(validation.FailureReason);
        }

        // Use TimeManager to advance time
        if (_forceAdvance)
        {
            // For forced advances (like sleeping), use AdvanceTime which bypasses active hour checks
            gameWorld.TimeManager.AdvanceTime(_hours);
        }
        else
        {
            // For normal actions, use SpendHours which enforces active hour limits
            if (!gameWorld.TimeManager.SpendHours(_hours))
            {
                return CommandResult.Failure("Failed to advance time");
            }
        }

        // Get new state from TimeManager
        int newDay = gameWorld.TimeManager.GetCurrentDay();
        int newHour = gameWorld.TimeManager.GetCurrentTimeHours();
        TimeBlocks newTimeBlock = gameWorld.TimeManager.GetCurrentTimeBlock();
        bool crossedDay = false; // TODO: Track day crossing in TimeManager

        // Add to event log
        string message = $"Time advanced by {_hours} hour(s) for {_reason}";

        gameWorld.SystemMessages.Add(new SystemMessage(message));

        return CommandResult.Success(
            message,
            new
            {
                NewDay = newDay,
                NewHour = newHour,
                NewTimeBlock = newTimeBlock.ToString(),
                CrossedDay = crossedDay
            }
        );
    }

    private async Task TriggerDayTransition(GameWorld gameWorld, int daysAdvanced)
    {
        // This would integrate with the DayTransitionOrchestrator from the architectural plan
        // For now, we'll just handle basic morning activities

        for (int i = 0; i < daysAdvanced; i++)
        {
            // Add morning message
            gameWorld.SystemMessages.Add(new SystemMessage($"A new day begins. Day {gameWorld.CurrentDay}"));
        }
    }
}