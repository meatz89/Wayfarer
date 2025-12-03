/// <summary>
/// Manages all segment-based time operations in the game.
/// Single source of truth for time state and progression using segments.
/// </summary>
public class TimeManager
{
    private readonly TimeModel _timeModel;
    private readonly MessageSystem _messageSystem;

    public TimeModel TimeModel => _timeModel;

    public int CurrentDay => _timeModel.CurrentDay;

    public int CurrentSegment => _timeModel.CurrentSegment;

    public int SegmentsRemainingInDay => _timeModel.SegmentsRemainingInDay;

    public int SegmentsRemainingInBlock => _timeModel.SegmentsRemainingInBlock;

    public TimeBlocks CurrentTimeBlock => _timeModel.CurrentTimeBlock;

    public TimeManager(
        TimeModel timeModel,
        MessageSystem messageSystem)
    {
        _timeModel = timeModel;
        _messageSystem = messageSystem;

        // Events removed per architecture guidelines - handle results directly
    }

    /// <summary>
    /// Checks if an action requiring specified segments can be performed.
    /// HIGHLANDER: segmentsRequired REQUIRED - comes from Consequence.TimeSegments
    /// </summary>
    public bool CanPerformAction(int segmentsRequired)
    {
        return _timeModel.CanPerformAction(segmentsRequired);
    }

    public int GetCurrentDay()
    {
        return CurrentDay;
    }

    public TimeBlocks GetCurrentTimeBlock()
    {
        return CurrentTimeBlock;
    }

    public void AdvanceSegments(int segments)
    {
        if (segments <= 0) return;

        // Show time passing message
        string timeDescription = GetSegmentPassingDescription(segments);
        _messageSystem.AddSystemMessage(
            $"{timeDescription}",
            SystemMessageTypes.Info,
            MessageCategory.TimeProgression);

        // Advance segments in the time model
        TimeAdvancementResult result = _timeModel.AdvanceSegments(segments);

        // Handle time block and day transitions
        HandleTimeAdvancement(result);
    }

    /// <summary>
    /// Jumps to the next time period (like work or rest actions).
    /// Advances to the first segment of the next time block.
    /// </summary>
    public void JumpToNextPeriod()
    {
        _messageSystem.AddSystemMessage(
            "Moving to the next time period...",
            SystemMessageTypes.Info,
            MessageCategory.TimeProgression);

        TimeAdvancementResult result = _timeModel.JumpToNextPeriod();

        HandleTimeAdvancement(result);
    }

    private string GetSegmentPassingDescription(int segments)
    {
        return segments switch
        {
            1 => "A segment passes...",
            2 => "Two segments pass...",
            3 => "Three segments pass...",
            4 => "Four segments pass (full period)...",
            _ => $"{segments} segments pass..."
        };
    }

    /// <summary>
    /// Simple segment advancement for basic actions.
    /// </summary>
    public async Task<bool> SpendSegments(int segments, string description)
    {
        if (!CanPerformAction(segments))
        {
            _messageSystem.AddSystemMessage(
                $"Not enough segments remaining. Need {segments} segments, have {SegmentsRemainingInDay}.",
                SystemMessageTypes.Warning,
                null);
            return false;
        }

        AdvanceSegments(segments);

        if (!string.IsNullOrEmpty(description))
        {
            string segmentDesc = segments == 1 ? "1 segment" : $"{segments} segments";
            _messageSystem.AddSystemMessage(
                $"{description}: {segmentDesc}",
                SystemMessageTypes.Info,
                MessageCategory.TimeProgression);
        }

        return true;
    }

    /// <summary>
    /// Gets a description of the current time state using segments.
    /// </summary>
    public string GetTimeDescription()
    {
        return _timeModel.GetTimeDescription();
    }

    /// <summary>
    /// Gets a human-readable segment display string.
    /// Format: "AFTERNOON ●●○○ [2/4]"
    /// </summary>
    public string GetSegmentDisplay()
    {
        return _timeModel.GetSegmentDisplay();
    }

    /// <summary>
    /// Gets formatted time display with day name and segment status.
    /// Returns format like "MON AFTERNOON ●●○○"
    /// </summary>
    public string GetFormattedTimeDisplay()
    {
        int day = GetCurrentDay();
        // Day 1 = Monday, so subtract 1 to get correct array index
        string dayName = new[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" }[(day - 1) % 7];

        string timeBlock = CurrentTimeBlock.ToString().ToUpper();
        string segmentDots = _timeModel.CurrentState.GetCompactSegmentDisplay();

        return $"{dayName} {timeBlock} {segmentDots}";
    }

    // Handle time advancement result directly
    private void HandleTimeAdvancement(TimeAdvancementResult result)
    {
        // Time block transitions
        if (result.CrossedTimeBlock)
        {
            _messageSystem.AddSystemMessage(
                $"Entering {result.NewTimeBlock.ToString().ToLower()} period",
                SystemMessageTypes.Info,
                MessageCategory.TimeProgression);
        }

        // Day transitions
        if (result.CrossedDayBoundary)
        {
            _messageSystem.AddSystemMessage(
                $"Day {result.NewState.CurrentDay} begins",
                SystemMessageTypes.Info,
                MessageCategory.TimeProgression);
        }
    }

}