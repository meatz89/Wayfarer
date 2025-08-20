using Microsoft.AspNetCore.Components;

/// <summary>
/// Helper class for unified time display across all UI components
/// </summary>
public static class TimeDisplayHelper
{
    /// <summary>
    /// Gets the formatted time display from TimeManager
    /// </summary>
    public static string GetFormattedTime(ITimeManager timeManager)
    {
        if (timeManager == null)
            return "Time Unknown";

        try
        {
            return timeManager.GetFormattedTimeDisplay();
        }
        catch
        {
            return "Time Error";
        }
    }
}