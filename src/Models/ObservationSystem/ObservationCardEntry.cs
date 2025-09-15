using System;

/// <summary>
/// Tracks when each observation card was acquired for expiration
/// </summary>
public class ObservationCardEntry
{
    public ObservationCard Card { get; set; }
    public DateTime AcquiredAt { get; set; } // Game time when acquired
    public int DayAcquired { get; set; } // Game day when acquired
    public TimeBlocks TimeBlockAcquired { get; set; } // Time block when acquired

    /// <summary>
    /// Check if this card has expired (96 segments based on decay rate)
    /// </summary>
    public bool IsExpired(int currentDay, TimeBlocks currentTimeBlock)
    {
        // Calculate segments elapsed in game time
        int daysElapsed = currentDay - DayAcquired;
        int timeBlocksElapsed = (daysElapsed * 6) + (currentTimeBlock - TimeBlockAcquired);
        int segmentsElapsed = timeBlocksElapsed * 8; // Each time block is 8 segments

        // Default 16 segment expiration
        return segmentsElapsed >= 16;
    }
}