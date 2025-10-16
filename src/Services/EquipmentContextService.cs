using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Domain service for equipment context matching
/// Pure domain logic - determines which equipment applies to which obstacles based on context tags
/// No external dependencies
/// </summary>
public static class EquipmentContextService
{
    /// <summary>
    /// Find all equipment in player inventory that matches obstacle contexts
    /// Returns list of (Equipment, matching contexts) tuples for UI display
    /// </summary>
    public static List<EquipmentContextMatch> FindMatchingEquipment(
        List<Equipment> availableEquipment,
        Obstacle obstacle)
    {
        List<EquipmentContextMatch> matches = new List<EquipmentContextMatch>();

        if (obstacle == null || obstacle.Contexts == null || obstacle.Contexts.Count == 0)
            return matches;

        foreach (Equipment equipment in availableEquipment)
        {
            if (equipment.ApplicableContexts == null || equipment.ApplicableContexts.Count == 0)
                continue;

            // Find intersection of equipment contexts and obstacle contexts
            List<ObstacleContext> matchingContexts = equipment.ApplicableContexts
                .Intersect(obstacle.Contexts)
                .ToList();

            if (matchingContexts.Count > 0)
            {
                matches.Add(new EquipmentContextMatch
                {
                    Equipment = equipment,
                    MatchingContexts = matchingContexts,
                    IntensityReduction = equipment.IntensityReduction
                });
            }
        }

        return matches;
    }

    /// <summary>
    /// Calculate total intensity reduction from all matching equipment
    /// Multiple equipment pieces stack additively
    /// </summary>
    public static int CalculateTotalIntensityReduction(
        List<Equipment> availableEquipment,
        Obstacle obstacle)
    {
        List<EquipmentContextMatch> matches = FindMatchingEquipment(availableEquipment, obstacle);
        return matches.Sum(m => m.IntensityReduction);
    }

    /// <summary>
    /// Check if any equipment matches obstacle contexts
    /// </summary>
    public static bool HasMatchingEquipment(
        List<Equipment> availableEquipment,
        Obstacle obstacle)
    {
        return FindMatchingEquipment(availableEquipment, obstacle).Count > 0;
    }
}

/// <summary>
/// Result of equipment-obstacle context matching
/// Contains equipment, matching contexts, and intensity reduction value
/// </summary>
public class EquipmentContextMatch
{
    public Equipment Equipment { get; set; }
    public List<ObstacleContext> MatchingContexts { get; set; } = new List<ObstacleContext>();
    public int IntensityReduction { get; set; }
}
