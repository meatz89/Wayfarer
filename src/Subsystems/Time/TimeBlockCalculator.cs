using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.TimeSubsystem
{
    /// <summary>
    /// Calculates time block transitions and availability windows.
    /// </summary>
    public class TimeBlockCalculator
    {
        /// <summary>
        /// Get the starting hour for a time block.
        /// </summary>
        public int GetTimeBlockStartHour(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => 4,
                TimeBlocks.Morning => 8,
                TimeBlocks.Afternoon => 12,
                TimeBlocks.Evening => 17,
                TimeBlocks.Night => 20,
                TimeBlocks.LateNight => 22,
                _ => 0
            };
        }
        
        /// <summary>
        /// Get the ending hour for a time block.
        /// </summary>
        public int GetTimeBlockEndHour(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => 8,
                TimeBlocks.Morning => 12,
                TimeBlocks.Afternoon => 17,
                TimeBlocks.Evening => 20,
                TimeBlocks.Night => 22,
                TimeBlocks.LateNight => 4, // Wraps to next day
                _ => 24
            };
        }
        
        /// <summary>
        /// Calculate hours until a specific time block from current position.
        /// </summary>
        public int CalculateHoursUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentHour)
        {
            int targetStartHour = GetTimeBlockStartHour(target);
            
            // If target is later today
            if (targetStartHour > currentHour)
            {
                return targetStartHour - currentHour;
            }
            
            // Target is tomorrow
            return (24 - currentHour) + targetStartHour;
        }
        
        /// <summary>
        /// Get the next available time block from a list of available times.
        /// </summary>
        public TimeBlocks? GetNextAvailableTimeBlock(TimeBlocks current, List<TimeBlocks> availableTimes)
        {
            if (availableTimes.Count == 0) return null;
            
            TimeBlocks[] timeOrder = new[] 
            { 
                TimeBlocks.Dawn, 
                TimeBlocks.Morning, 
                TimeBlocks.Afternoon, 
                TimeBlocks.Evening, 
                TimeBlocks.Night,
                TimeBlocks.LateNight
            };
            
            int currentIndex = Array.IndexOf(timeOrder, current);
            
            // Look for next available time after current
            for (int i = currentIndex + 1; i < timeOrder.Length; i++)
            {
                if (availableTimes.Contains(timeOrder[i]))
                {
                    return timeOrder[i];
                }
            }
            
            // Wrap around to tomorrow
            for (int i = 0; i <= currentIndex; i++)
            {
                if (availableTimes.Contains(timeOrder[i]))
                {
                    return timeOrder[i];
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Get display name for a time block.
        /// </summary>
        public string GetTimeBlockDisplayName(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => "Dawn (4-8 AM)",
                TimeBlocks.Morning => "Morning (8 AM-12 PM)",
                TimeBlocks.Afternoon => "Afternoon (12-5 PM)",
                TimeBlocks.Evening => "Evening (5-8 PM)",
                TimeBlocks.Night => "Night (8 PM-10 PM)",
                TimeBlocks.LateNight => "Late Night (10 PM-4 AM)",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// Get narrative description for waiting until a time block.
        /// </summary>
        public string GetWaitingNarrative(TimeBlocks targetTime)
        {
            return targetTime switch
            {
                TimeBlocks.Dawn => "You wait as the first light breaks over the horizon.",
                TimeBlocks.Morning => "The morning sun climbs higher as time passes.",
                TimeBlocks.Afternoon => "The day wears on toward afternoon.",
                TimeBlocks.Evening => "Shadows lengthen as evening approaches.",
                TimeBlocks.Night => "Darkness falls across the town.",
                TimeBlocks.LateNight => "The deep of night settles in.",
                _ => "Time passes..."
            };
        }
    }
}