using System;
using Wayfarer.Core.Repositories;

namespace Wayfarer.GameState;

/// <summary>
/// Adapter that allows the old TimeManager to work with the new UnifiedTimeService.
/// This enables gradual migration without breaking existing code.
/// </summary>
public class TimeManagerRefactoringAdapter : ITimeManager
{
    private readonly UnifiedTimeService _unifiedTimeService;
    private readonly Player _player;
    private readonly WorldState _worldState;

    public int CurrentTimeHours => _unifiedTimeService.CurrentHour;
    public int HoursRemaining => _unifiedTimeService.ActiveHoursRemaining;

    public TimeManagerRefactoringAdapter(
        UnifiedTimeService unifiedTimeService,
        Player player,
        WorldState worldState)
    {
        _unifiedTimeService = unifiedTimeService ?? throw new ArgumentNullException(nameof(unifiedTimeService));
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _worldState = worldState ?? throw new ArgumentNullException(nameof(worldState));

        // Keep WorldState in sync
        _unifiedTimeService.TimeModel.TimeAdvanced += OnTimeAdvanced;
    }

    public int GetCurrentTimeHours()
    {
        return _unifiedTimeService.CurrentHour;
    }

    public int GetCurrentDay()
    {
        return _unifiedTimeService.CurrentDay;
    }

    public TimeBlocks GetCurrentTimeBlock()
    {
        return _unifiedTimeService.CurrentTimeBlock;
    }

    public bool CanPerformAction(int hoursRequired = 1)
    {
        return _unifiedTimeService.CanPerformAction(hoursRequired);
    }

    /// <summary>
    /// Legacy method - delegates to UnifiedTimeService.
    /// </summary>
    public bool SpendHours(int hours)
    {
        if (hours <= 0) return false;
        
        var task = _unifiedTimeService.SpendTime(hours, "Action");
        task.Wait(); // Synchronous for compatibility
        return task.Result;
    }

    /// <summary>
    /// Legacy method - delegates to UnifiedTimeService.
    /// </summary>
    public void AdvanceTime(int hours)
    {
        if (hours <= 0) return;

        // Use a transaction that doesn't require active hours
        var transaction = _unifiedTimeService.CreateTransaction()
            .WithHours(hours, "Time advancement")
            .RequireActiveHours(false);

        transaction.Execute();
    }

    /// <summary>
    /// Legacy method - delegates to UnifiedTimeService.
    /// </summary>
    public void StartNewDay()
    {
        var task = _unifiedTimeService.AdvanceToNextDay();
        task.Wait(); // Synchronous for compatibility
    }

    /// <summary>
    /// Legacy method - delegates to UnifiedTimeService.
    /// </summary>
    public void SetNewTime(int hours)
    {
        // This is a bit tricky - we need to calculate the difference
        var currentHour = _unifiedTimeService.CurrentHour;
        var difference = hours - currentHour;

        if (difference > 0)
        {
            AdvanceTime(difference);
        }
        else if (difference < 0)
        {
            // Can't go backwards in time, advance to next day at target hour
            var hoursToAdvance = (24 - currentHour) + hours;
            AdvanceTime(hoursToAdvance);
        }
    }

    // Keep WorldState synchronized
    private void OnTimeAdvanced(object sender, TimeAdvancementResult e)
    {
        _worldState.CurrentTimeBlock = e.NewState.CurrentTimeBlock;
        _worldState.CurrentDay = e.NewState.CurrentDay;
    }
}