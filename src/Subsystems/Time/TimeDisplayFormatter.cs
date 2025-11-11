/// <summary>
/// Formats time for display in the UI.
/// </summary>
public class TimeDisplayFormatter
{
    private readonly TimeManager _timeManager;

    public TimeDisplayFormatter(TimeManager timeManager)
    {
        _timeManager = timeManager;
    }

    /// <summary>
    /// Get formatted time display with day name and time.
    /// Returns format like "MON 3:30 PM"
    /// </summary>
    public string GetFormattedTimeDisplay()
    {
        return _timeManager.GetFormattedTimeDisplay();
    }

    /// <summary>
    /// Get simple time string using segments.
    /// </summary>
    public string GetTimeString()
    {
        return _timeManager.GetSegmentDisplay();
    }

    /// <summary>
    /// Get descriptive time text.
    /// </summary>
    public string GetTimeDescription()
    {
        return _timeManager.GetTimeDescription();
    }

    /// <summary>
    /// Format duration in segments for display.
    /// </summary>
    public string FormatDuration(int segments)
    {
        return FormatSegments(segments);
    }

    /// <summary>
    /// Format segments for display (alias for FormatSegments).
    /// </summary>
    public string FormatMinutes(int segments)
    {
        return FormatSegments(segments);
    }

    /// <summary>
    /// Get day name from day number.
    /// </summary>
    public string GetDayName(int day)
    {
        string[] dayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        return dayNames[(day - 1) % 7];
    }

    /// <summary>
    /// Get short day name from day number.
    /// </summary>
    public string GetShortDayName(int day)
    {
        string[] dayNames = { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
        return dayNames[(day - 1) % 7];
    }

    /// <summary>
    /// Format segments for display.
    /// </summary>
    public string FormatSegments(int segments)
    {
        if (segments == 0) return "No time";
        if (segments == 1) return "1 segment";
        if (segments <= 4) return $"{segments} segments";

        // Convert larger segment counts to periods
        int periods = segments / 4;
        int remainingSegments = segments % 4;

        if (remainingSegments == 0)
        {
            return periods == 1 ? "1 period" : $"{periods} periods";
        }

        string periodPart = periods == 1 ? "1 period" : $"{periods} periods";
        string segmentPart = remainingSegments == 1 ? "1 segment" : $"{remainingSegments} segments";

        return $"{periodPart}, {segmentPart}";
    }
}
