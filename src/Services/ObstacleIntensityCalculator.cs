using System;
using System.Collections.Generic;

/// <summary>
/// Domain service for calculating effective obstacle intensity with equipment context matching
/// Pure domain logic - calculates modified obstacle intensity based on player equipment
/// No external dependencies
/// </summary>
public static class ObstacleIntensityCalculator
{
    /// <summary>
    /// Calculate effective obstacle intensity after applying equipment context reductions
    /// Equipment that matches obstacle contexts reduces intensity baseline
    /// Returns modified obstacle values for tactical engagement calculation
    /// </summary>
    public static ObstacleIntensity CalculateEffectiveIntensity(
        Obstacle obstacle,
        List<Equipment> availableEquipment)
    {
        if (obstacle == null)
            throw new ArgumentNullException(nameof(obstacle));

        // Calculate total intensity reduction from matching equipment
        int intensityReduction = EquipmentContextService.CalculateTotalIntensityReduction(
            availableEquipment ?? new List<Equipment>(),
            obstacle);

        // Apply reduction to obstacle intensity, floor at 0
        int effectiveIntensity = Math.Max(0, obstacle.Intensity - intensityReduction);

        return new ObstacleIntensity
        {
            ObstacleId = obstacle.Id,
            ObstacleName = obstacle.Name,
            BaseIntensity = obstacle.Intensity,
            EffectiveIntensity = effectiveIntensity,
            IntensityReduction = intensityReduction,
            MatchingEquipment = EquipmentContextService.FindMatchingEquipment(
                availableEquipment ?? new List<Equipment>(),
                obstacle)
        };
    }

    /// <summary>
    /// Check if obstacle is effectively cleared (intensity reduced to 0)
    /// </summary>
    public static bool IsEffectivelyCleared(ObstacleIntensity intensity)
    {
        return intensity.EffectiveIntensity == 0;
    }
}

/// <summary>
/// Result of obstacle intensity calculation with equipment context matching applied
/// Contains both base obstacle intensity and effective intensity after equipment reductions
/// </summary>
public class ObstacleIntensity
{
    public string ObstacleId { get; set; }
    public string ObstacleName { get; set; }

    // Base value from obstacle definition
    public int BaseIntensity { get; set; }

    // Effective value after equipment context reductions
    public int EffectiveIntensity { get; set; }

    // Total intensity reduction applied
    public int IntensityReduction { get; set; }

    // Equipment that contributed to reduction
    public List<EquipmentContextMatch> MatchingEquipment { get; set; } = new List<EquipmentContextMatch>();
}
