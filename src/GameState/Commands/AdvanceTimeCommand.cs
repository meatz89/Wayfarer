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
        // Commands don't validate time constraints - that's handled by the executing service
        // This command will request time advancement
        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        // Commands return metadata about what needs to be done
        // The executing service will handle actual time advancement

        // Add to event log
        string message = $"Time advancement requested: {_hours} hour(s) for {_reason}";
        gameWorld.SystemMessages.Add(new SystemMessage(message));

        return CommandResult.Success(
            message,
            new
            {
                TimeCost = _hours,
                Reason = _reason,
                ForceAdvance = _forceAdvance
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