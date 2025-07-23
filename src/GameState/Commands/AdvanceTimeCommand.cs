using System;
using System.Threading.Tasks;
using Wayfarer.GameState.StateContainers;

namespace Wayfarer.GameState.Commands;

/// <summary>
/// Command to advance game time by a specified number of hours.
/// This command does not support undo as time advancement triggers cascading effects.
/// </summary>
public class AdvanceTimeCommand : BaseGameCommand
{
    private readonly int _hours;
    private readonly string _reason;
    private readonly bool _forceAdvance;
    
    public override string Description => $"Advance time by {_hours} hour(s) for {_reason}";
    public override bool CanUndo => false; // Time advancement cannot be undone
    
    public AdvanceTimeCommand(int hours, string reason = "activity", bool forceAdvance = false)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be positive", nameof(hours));
            
        _hours = hours;
        _reason = reason;
        _forceAdvance = forceAdvance;
    }
    
    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Create TimeState from current values
        var currentTime = new TimeState(gameWorld.CurrentDay, gameWorld.TimeManager.CurrentTimeHours);
        
        // Check if we can spend active hours (unless forced, like sleeping)
        if (!_forceAdvance && !currentTime.CanSpendActiveHours(_hours))
        {
            var remaining = currentTime.ActiveHoursRemaining;
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
        var validation = CanExecute(gameWorld);
        if (!validation.IsValid && !_forceAdvance)
        {
            return CommandResult.FailureResult(validation.FailureReason);
        }
        
        // Create TimeState from current values
        var currentTime = new TimeState(gameWorld.CurrentDay, gameWorld.TimeManager.CurrentTimeHours);
        
        // Advance time atomically
        var result = currentTime.AdvanceTime(_hours);
        
        // Apply the new state to the game world
        gameWorld.CurrentDay = result.NewState.CurrentDay;
        gameWorld.TimeManager.SetNewTime(result.NewState.CurrentHour);
        
        // Handle day transitions
        if (result.CrossedDayBoundary)
        {
            // Trigger morning activities
            await TriggerDayTransition(gameWorld, result.DaysAdvanced);
        }
        
        // Add to event log
        var message = $"Time advanced by {_hours} hour(s) for {_reason}";
        if (result.CrossedDayBoundary)
        {
            message += $" - New day: {result.NewState.CurrentDay}";
        }
        
        gameWorld.SystemMessages.Add(new SystemMessage
        {
            Message = message,
            Type = MessageType.Time,
            Timestamp = DateTime.UtcNow
        });
        
        return CommandResult.SuccessResult(
            message,
            new 
            { 
                NewDay = result.NewState.CurrentDay,
                NewHour = result.NewState.CurrentHour,
                NewTimeBlock = result.NewState.CurrentTimeBlock.ToString(),
                CrossedDay = result.CrossedDayBoundary
            }
        );
    }
    
    private async Task TriggerDayTransition(GameWorld gameWorld, int daysAdvanced)
    {
        // This would integrate with the DayTransitionOrchestrator from the architectural plan
        // For now, we'll just handle basic morning activities
        
        for (int i = 0; i < daysAdvanced; i++)
        {
            // Trigger letter queue morning activities
            if (gameWorld.NarrativeManager != null)
            {
                await gameWorld.NarrativeManager.OnNewDay();
            }
            
            // Add morning message
            gameWorld.SystemMessages.Add(new SystemMessage
            {
                Message = $"A new day begins. Day {gameWorld.CurrentDay}",
                Type = MessageType.DayTransition,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}