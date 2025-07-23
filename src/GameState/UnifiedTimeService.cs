using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wayfarer.GameState.TimeEffects;

namespace Wayfarer.GameState;

/// <summary>
/// Unified service that manages all time-related operations in the game.
/// Replaces the scattered time management across TimeManager, GameWorld, and WorldState.
/// </summary>
public class UnifiedTimeService
{
    private readonly TimeModel _timeModel;
    private readonly DayTransitionOrchestrator _dayTransitionOrchestrator;
    private readonly ILogger<UnifiedTimeService> _logger;
    private readonly MessageSystem _messageSystem;

    public TimeModel TimeModel => _timeModel;
    public int CurrentHour => _timeModel.CurrentHour;
    public int CurrentDay => _timeModel.CurrentDay;
    public TimeBlocks CurrentTimeBlock => _timeModel.CurrentTimeBlock;
    public int ActiveHoursRemaining => _timeModel.ActiveHoursRemaining;

    public UnifiedTimeService(
        TimeModel timeModel,
        DayTransitionOrchestrator dayTransitionOrchestrator,
        MessageSystem messageSystem,
        ILogger<UnifiedTimeService> logger)
    {
        _timeModel = timeModel ?? throw new ArgumentNullException(nameof(timeModel));
        _dayTransitionOrchestrator = dayTransitionOrchestrator ?? throw new ArgumentNullException(nameof(dayTransitionOrchestrator));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to time model events
        _timeModel.TimeAdvanced += OnTimeAdvanced;
        _timeModel.NewDayStarted += OnNewDayStarted;
    }

    /// <summary>
    /// Checks if an action requiring specified hours can be performed.
    /// </summary>
    public bool CanPerformAction(int hoursRequired)
    {
        return _timeModel.CanPerformAction(hoursRequired);
    }

    /// <summary>
    /// Creates a new time transaction for complex time-based operations.
    /// </summary>
    public TimeTransaction CreateTransaction()
    {
        return new TimeTransaction(_timeModel);
    }

    /// <summary>
    /// Simple time advancement for basic actions.
    /// </summary>
    public async Task<bool> SpendTime(int hours, string description = null)
    {
        if (!CanPerformAction(hours))
        {
            _messageSystem.AddSystemMessage(
                $"Not enough time remaining. Need {hours} hours, have {ActiveHoursRemaining}.", 
                SystemMessageTypes.Warning);
            return false;
        }

        var transaction = CreateTransaction()
            .WithHours(hours, description);

        var result = transaction.Execute();

        if (result.Success)
        {
            var timeDesc = hours == 1 ? "1 hour" : $"{hours} hours";
            _messageSystem.AddSystemMessage(
                $"⏱️ {description ?? "Time passed"}: {timeDesc}", 
                SystemMessageTypes.Info);
        }

        return result.Success;
    }

    /// <summary>
    /// Performs an action that costs both time and stamina.
    /// </summary>
    public async Task<bool> PerformTimedAction(
        Player player, 
        int hours, 
        int staminaCost, 
        string actionDescription)
    {
        var transaction = CreateTransaction()
            .WithHours(hours, actionDescription)
            .WithEffect(new StaminaCostEffect(player, staminaCost));

        var validation = transaction.CanExecute();
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                _messageSystem.AddSystemMessage(error, SystemMessageTypes.Danger);
            }
            return false;
        }

        var result = transaction.Execute();

        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"✓ {actionDescription} completed", 
                SystemMessageTypes.Success);
        }

        return result.Success;
    }

    /// <summary>
    /// Performs travel which involves time, stamina, and location change.
    /// </summary>
    public async Task<bool> Travel(
        Player player,
        Location destination,
        int travelHours,
        int staminaCost)
    {
        var transaction = CreateTransaction()
            .WithHours(travelHours, $"Travel to {destination.Name}")
            .WithEffect(new StaminaCostEffect(player, staminaCost))
            .WithEffect(new LocationChangeEffect(player, destination));

        var result = transaction.Execute();

        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"🚶 Arrived at {destination.Name} after {travelHours} hours of travel", 
                SystemMessageTypes.Success);
        }

        return result.Success;
    }

    /// <summary>
    /// Forces time to advance to the next day (for sleeping, etc).
    /// </summary>
    public async Task<DayTransitionResult> AdvanceToNextDay()
    {
        _logger.LogInformation("Advancing to next day");
        
        // Calculate hours until dawn
        var hoursUntilDawn = (24 - CurrentHour) + TimeModel.ACTIVE_DAY_START;
        
        // Create a transaction that doesn't require active hours
        var transaction = CreateTransaction()
            .WithHours(hoursUntilDawn, "Sleep until dawn")
            .RequireActiveHours(false);

        var result = transaction.Execute();

        if (!result.Success)
        {
            _logger.LogError("Failed to advance to next day: {Errors}", string.Join(", ", result.Errors));
            return null;
        }

        // The time model will have triggered NewDayStarted event, which calls our handler
        // Return the day transition result
        return await _dayTransitionOrchestrator.ProcessNewDay();
    }

    /// <summary>
    /// Waits for a specific number of hours without performing any action.
    /// </summary>
    public async Task<bool> Wait(int hours)
    {
        if (hours <= 0) return false;

        // Waiting can go past active hours but not past midnight
        if (CurrentHour + hours >= 24)
        {
            _messageSystem.AddSystemMessage(
                $"Cannot wait {hours} hours: would go past midnight", 
                SystemMessageTypes.Danger);
            return false;
        }

        var transaction = CreateTransaction()
            .WithHours(hours, "Wait")
            .RequireActiveHours(false); // Allow waiting into night

        var result = transaction.Execute();

        if (result.Success)
        {
            var timeDescription = hours == 1 ? "1 hour" : $"{hours} hours";
            _messageSystem.AddSystemMessage(
                $"Waited {timeDescription}. Time is now {_timeModel.GetTimeString()}", 
                SystemMessageTypes.Info);
        }

        return result.Success;
    }

    /// <summary>
    /// Gets a description of the current time state.
    /// </summary>
    public string GetTimeDescription()
    {
        return _timeModel.GetTimeDescription();
    }

    /// <summary>
    /// Gets the current time as a formatted string.
    /// </summary>
    public string GetTimeString()
    {
        return _timeModel.GetTimeString();
    }

    // Event handlers
    private void OnTimeAdvanced(object sender, TimeAdvancementResult e)
    {
        _logger.LogDebug("Time advanced by {Hours} hours to {NewTime}", 
            e.HoursAdvanced, 
            e.NewState.ToString());

        // Log time block transitions
        if (e.CrossedTimeBlock)
        {
            _logger.LogInformation("Time block changed from {OldBlock} to {NewBlock}", 
                e.OldTimeBlock, 
                e.NewTimeBlock);
        }
    }

    private async void OnNewDayStarted(object sender, NewDayEventArgs e)
    {
        _logger.LogInformation("New day started: Day {NewDay}", e.NewDay);
        
        // The day transition is handled by AdvanceToNextDay or the orchestrator
        // This is just for logging and any immediate notifications
        _messageSystem.AddSystemMessage(
            $"☀️ Dawn of Day {e.NewDay}", 
            SystemMessageTypes.Info);
    }
}