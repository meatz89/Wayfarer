using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.TimeSubsystem
{
    /// <summary>
    /// Calculates time block transitions and availability windows.
    /// </summary>
    public class TimeBlockCalculator
    {
        // Legacy hour-based methods have been replaced with segment-based equivalents

        /// <summary>
        /// Calculate segments until a specific time block from current position.
        /// This is the primary method for time calculations.
        /// </summary>
        public int CalculateTimeUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentSegment)
        {
            return CalculateSegmentsUntilTimeBlock(current, target, currentSegment);
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

        // Segment-based methods for the new time system

        /// <summary>
        /// Get the starting segment for a time block within the day.
        /// </summary>
        public int GetTimeBlockStartSegment(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => 0,
                TimeBlocks.Morning => 3,
                TimeBlocks.Afternoon => 7,
                TimeBlocks.Evening => 11,
                TimeBlocks.Night => 15,
                TimeBlocks.LateNight => 16,
                _ => 0
            };
        }

        /// <summary>
        /// Get the ending segment for a time block within the day.
        /// </summary>
        public int GetTimeBlockEndSegment(TimeBlocks timeBlock)
        {
            return timeBlock switch
            {
                TimeBlocks.Dawn => 2,
                TimeBlocks.Morning => 6,
                TimeBlocks.Afternoon => 10,
                TimeBlocks.Evening => 14,
                TimeBlocks.Night => 15,
                TimeBlocks.LateNight => 16,
                _ => 16
            };
        }

        /// <summary>
        /// Calculate segments until a specific time block from current position.
        /// </summary>
        public int CalculateSegmentsUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentSegment)
        {
            int targetStartSegment = GetTimeBlockStartSegment(target);
            int currentBlockStart = GetTimeBlockStartSegment(current);

            // If target is later today
            if (targetStartSegment > currentSegment)
            {
                return targetStartSegment - currentSegment;
            }

            // Target is tomorrow - calculate remaining segments today + segments to target tomorrow
            int segmentsRemainingToday = 16 - currentSegment;
            return segmentsRemainingToday + targetStartSegment;
        }
    }
}