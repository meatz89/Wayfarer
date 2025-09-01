using System;

namespace Wayfarer.Subsystems.TimeSubsystem
{
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
        /// Get simple time string.
        /// </summary>
        public string GetTimeString()
        {
            return _timeManager.GetTimeString();
        }
        
        /// <summary>
        /// Get descriptive time text.
        /// </summary>
        public string GetTimeDescription()
        {
            return _timeManager.GetTimeDescription();
        }
        
        /// <summary>
        /// Format duration in hours for display.
        /// </summary>
        public string FormatDuration(int hours)
        {
            if (hours == 0) return "No time";
            if (hours == 1) return "1 hour";
            if (hours < 24) return $"{hours} hours";
            
            int days = hours / 24;
            int remainingHours = hours % 24;
            
            if (remainingHours == 0)
            {
                return days == 1 ? "1 day" : $"{days} days";
            }
            
            string dayPart = days == 1 ? "1 day" : $"{days} days";
            string hourPart = remainingHours == 1 ? "1 hour" : $"{remainingHours} hours";
            
            return $"{dayPart}, {hourPart}";
        }
        
        /// <summary>
        /// Format minutes for display.
        /// </summary>
        public string FormatMinutes(int minutes)
        {
            if (minutes < 60)
                return $"{minutes} minutes";
            else if (minutes == 60)
                return "1 hour";
            else if (minutes < 120)
                return $"1 hour and {minutes - 60} minutes";
            else
            {
                int hours = minutes / 60;
                int mins = minutes % 60;
                if (mins == 0)
                    return $"{hours} hours";
                else
                    return $"{hours} hours and {mins} minutes";
            }
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
    }
}