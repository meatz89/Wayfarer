using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Public facade for all time-related operations.
/// Single entry point for time management, progression, and display.
/// </summary>
public class TimeFacade
{
    private readonly TimeManager _timeManager;
    private readonly TimeBlockCalculator _timeBlockCalculator;
    private readonly TimeProgressionManager _timeProgressionManager;
    private readonly TimeDisplayFormatter _timeDisplayFormatter;
    private readonly GameWorld _gameWorld;

    public TimeFacade(
        TimeManager timeManager,
        TimeBlockCalculator timeBlockCalculator,
        TimeProgressionManager timeProgressionManager,
        TimeDisplayFormatter timeDisplayFormatter,
        GameWorld gameWorld)
    {
        _timeManager = timeManager;
        _timeBlockCalculator = timeBlockCalculator;
        _timeProgressionManager = timeProgressionManager;
        _timeDisplayFormatter = timeDisplayFormatter;
        _gameWorld = gameWorld;
    }

    // ========== TIME STATE ==========

    public int GetCurrentDay()
    {
        return _gameWorld.CurrentDay;
    }

    public int GetCurrentSegment()
    {
        return _timeManager.CurrentSegment;
    }

    public int GetSegmentsInCurrentPeriod()
    {
        return _timeManager.TimeModel.CurrentState.SegmentsInCurrentBlock;
    }

    public TimeBlocks GetCurrentTimeBlock()
    {
        return _timeManager.CurrentTimeBlock;
    }

    public int GetSegmentsRemainingInDay()
    {
        return _timeManager.SegmentsRemainingInDay;
    }

    public TimeInfo GetTimeInfo()
    {
        return new TimeInfo(
            GetCurrentTimeBlock(),
            GetSegmentsRemainingInDay(),
            GetCurrentDay(),
            _timeManager.GetSegmentDisplay());
    }

    // ========== TIME PROGRESSION ==========

    public TimeBlocks AdvanceSegments(int segments)
    {
        int oldSegment = _timeManager.TimeModel.CurrentState.TotalSegmentsElapsed;
        TimeBlocks result = _timeProgressionManager.AdvanceSegments(segments);
        int newSegment = _timeManager.TimeModel.CurrentState.TotalSegmentsElapsed;

        // Check for deadline failures when crossing day boundary
        if (_gameWorld.CurrentDay != _timeManager.TimeModel.CurrentState.CurrentDay)
        {
            CheckAndProcessDeadlineFailures(newSegment);
        }

        return result;
    }

    private void CheckAndProcessDeadlineFailures(int currentSegment)
    {
        List<string> expiredObligations = _gameWorld.CheckDeadlines(currentSegment);

        foreach (string obligationId in expiredObligations)
        {
            _gameWorld.ApplyDeadlineConsequences(obligationId);
        }
    }

    public TimeBlocks JumpToNextPeriod()
    {
        return _timeProgressionManager.JumpToNextPeriod();
    }

    public int WaitUntilNextTimeBlock()
    {
        TimeBlocks current = GetCurrentTimeBlock();
        TimeBlocks next = GetNextTimeBlock(current);
        return _timeProgressionManager.WaitUntilTimeBlock(next, _timeBlockCalculator);
    }

    public async Task<bool> SpendSegments(int segments, string description)
    {
        return await _timeManager.SpendSegments(segments, description);
    }

    public bool CanPerformAction(int segmentsRequired)
    {
        return _timeProgressionManager.CanPerformAction(segmentsRequired);
    }

    // ========== TIME BLOCK CALCULATIONS ==========

    public int GetTimeBlockStartSegment(TimeBlocks timeBlock)
    {
        return _timeBlockCalculator.GetTimeBlockStartSegment(timeBlock);
    }

    public int GetTimeBlockEndSegment(TimeBlocks timeBlock)
    {
        return _timeBlockCalculator.GetTimeBlockEndSegment(timeBlock);
    }

    public int CalculateSegmentsUntilTimeBlock(TimeBlocks target)
    {
        return _timeBlockCalculator.CalculateSegmentsUntilTimeBlock(
            GetCurrentTimeBlock(),
            target,
            GetCurrentSegment());
    }

    public TimeBlocks? GetNextAvailableTimeBlock(List<TimeBlocks> availableTimes)
    {
        return _timeBlockCalculator.GetNextAvailableTimeBlock(
            GetCurrentTimeBlock(),
            availableTimes);
    }

    public string GetTimeBlockDisplayName(TimeBlocks timeBlock)
    {
        return _timeBlockCalculator.GetTimeBlockDisplayName(timeBlock);
    }

    public string GetWaitingNarrative(TimeBlocks targetTime)
    {
        return _timeBlockCalculator.GetWaitingNarrative(targetTime);
    }

    private TimeBlocks GetNextTimeBlock(TimeBlocks current)
    {
        return current switch
        {
            TimeBlocks.Morning => TimeBlocks.Midday,
            TimeBlocks.Midday => TimeBlocks.Afternoon,
            TimeBlocks.Afternoon => TimeBlocks.Evening,
            TimeBlocks.Evening => TimeBlocks.Morning, // Sleep wraps to next morning
            _ => TimeBlocks.Morning
        };
    }

    // ========== TIME DISPLAY ==========

    public string GetFormattedTimeDisplay()
    {
        return _timeDisplayFormatter.GetFormattedTimeDisplay();
    }

    public string GetTimeString()
    {
        return _timeDisplayFormatter.GetTimeString();
    }

    public string GetTimeDescription()
    {
        return _timeDisplayFormatter.GetTimeDescription();
    }

    public string FormatDuration(int segments)
    {
        return _timeDisplayFormatter.FormatDuration(segments);
    }

    public string FormatSegments(int segments)
    {
        return _timeDisplayFormatter.FormatSegments(segments);
    }

    public string GetDayName(int day)
    {
        return _timeDisplayFormatter.GetDayName(day);
    }

    public string GetShortDayName(int day)
    {
        return _timeDisplayFormatter.GetShortDayName(day);
    }

    // ========== AVAILABILITY WINDOWS ==========

    public string GetNextAvailableTimeDisplay(List<TimeBlocks> availableTimes)
    {
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(availableTimes);
        if (!nextTime.HasValue) return "Not available today";

        int segmentsUntil = CalculateSegmentsUntilTimeBlock(nextTime.Value);
        string timeBlockName = GetTimeBlockDisplayName(nextTime.Value);

        if (segmentsUntil == 0)
        {
            return $"Available now during {timeBlockName}";
        }
        else if (segmentsUntil == 1)
        {
            return $"Available in 1 segment at {timeBlockName}";
        }
        else
        {
            return $"Available in {segmentsUntil} segments at {timeBlockName}";
        }
    }

    public bool IsTimeBlockAvailable(TimeBlocks timeBlock, List<TimeBlocks> availableTimes)
    {
        return availableTimes.Contains(timeBlock);
    }

    public bool IsCurrentlyAvailable(List<TimeBlocks> availableTimes)
    {
        return IsTimeBlockAvailable(GetCurrentTimeBlock(), availableTimes);
    }

    // ========== DAY-END PROCESSING ==========

    /// <summary>
    /// End the current day - process deadlines, restore resources, generate summary
    /// Called explicitly when player chooses to rest/sleep
    /// </summary>
    public DayEndReport EndDay()
    {
        Player player = _gameWorld.GetPlayer();
        int currentSegment = GetCurrentSegment();

        DayEndReport report = new DayEndReport();

        // 1. Check for expired obligations (deadlines)
        List<string> expiredObligationIds = _gameWorld.CheckDeadlines(currentSegment);

        // 2. Apply deadline consequences and build failure report
        foreach (string obligationId in expiredObligationIds)
        {
            Investigation investigation = _gameWorld.Investigations.FirstOrDefault(i => i.Id == obligationId);
            if (investigation == null) continue;

            NPC patron = null;
            if (!string.IsNullOrEmpty(investigation.PatronNpcId))
            {
                patron = _gameWorld.NPCs.FirstOrDefault(n => n.ID == investigation.PatronNpcId);
            }

            int cubesBeforeConsequence = patron?.StoryCubes ?? 0;

            // Apply consequences
            _gameWorld.ApplyDeadlineConsequences(obligationId);

            int cubesAfterConsequence = patron?.StoryCubes ?? 0;

            report.FailedObligations.Add(new FailedObligationInfo
            {
                ObligationName = investigation.Name,
                PatronName = patron?.Name ?? "Unknown",
                CubesRemoved = cubesBeforeConsequence - cubesAfterConsequence
            });
        }

        // 3. Restore resources (Focus and Stamina only - Health does NOT auto-recover)
        player.Focus = 6; // Hardcoded max per design
        player.Stamina = player.MaxStamina;

        // 4. Build current resource snapshot
        report.CurrentResources = new ResourceSnapshot
        {
            Health = player.Health,
            Focus = player.Focus,
            Stamina = player.Stamina,
            Coins = player.Coins
        };

        // NOTE: CoinsEarned, CoinsSpent, CompletedObligations, NewEquipment, StatsIncreased, CubesGained
        // are NOT tracked by TimeFacade - these require tracking throughout the day
        // This would require day-scoped state tracking which violates stateless facade principle
        // For now, these fields remain empty - can be populated by caller if needed

        return report;
    }

    // ========== LEGACY COMPATIBILITY METHODS ==========
}
