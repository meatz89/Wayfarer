using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for calculating final difficulty based on base difficulty and player state
/// Replaces GoalRequirementsChecker (which was boolean gate system)
/// No boolean gates: All goals always visible, difficulty varies transparently
/// </summary>
public class DifficultyCalculationService
{
    private readonly GameWorld _gameWorld;

    public DifficultyCalculationService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Calculate final difficulty for a goal
    /// Returns base difficulty plus/minus modifiers
    /// Goal ALWAYS visible regardless of difficulty
    /// </summary>
    public DifficultyResult CalculateDifficulty(Goal goal, ItemRepository itemRepository)
    {
        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        Player player = _gameWorld.GetPlayer();
        int finalDifficulty = goal.BaseDifficulty;
        List<string> appliedModifiers = new List<string>();
        List<string> unappliedModifiers = new List<string>();

        foreach (DifficultyModifier mod in goal.DifficultyModifiers)
        {
            if (CheckModifier(mod, player, goal, itemRepository))
            {
                finalDifficulty += mod.Effect;  // Usually negative (reduction)
                appliedModifiers.Add(FormatModifier(mod, true));
            }
            else
            {
                unappliedModifiers.Add(FormatModifier(mod, false));
            }
        }

        return new DifficultyResult
        {
            BaseDifficulty = goal.BaseDifficulty,
            FinalDifficulty = Math.Max(0, finalDifficulty),
            AppliedModifiers = appliedModifiers,
            UnappliedModifiers = unappliedModifiers
        };
    }

    /// <summary>
    /// Check if modifier threshold is met
    /// NO ID MATCHING: Only mechanical properties and numerical resources
    /// </summary>
    private bool CheckModifier(DifficultyModifier mod, Player player, Goal goal, ItemRepository itemRepository)
    {
        switch (mod.Type)
        {
            case ModifierType.Understanding:
                // Global Mental expertise (0-10)
                return player.Understanding >= mod.Threshold;

            case ModifierType.Mastery:
                // Physical expertise per challenge type (uses DeckId, not Type)
                int mastery = player.MasteryTokens.GetMastery(mod.Context);
                return mastery >= mod.Threshold;

            case ModifierType.Familiarity:
                // Location understanding (0-3 per Location)
                // Uses Location ID from the goal's placement
                if (string.IsNullOrEmpty(goal.PlacementLocationId)) return false;
                int familiarity = player.GetLocationFamiliarity(goal.PlacementLocationId);
                return familiarity >= mod.Threshold;

            case ModifierType.ConnectionTokens:
                // NPC relationship strength (0-15 per NPC)
                // NO ID MATCHING: Uses goal's PlacementNpcId (mechanical property)
                if (string.IsNullOrEmpty(goal.PlacementNpcId)) return false;
                int tokens = player.NPCTokens.GetTokenCount(goal.PlacementNpcId, ConnectionType.Trust);
                return tokens >= mod.Threshold;

            case ModifierType.ObstacleProperty:
                // Check obstacle property threshold
                Obstacle obstacle = FindParentObstacle(goal);
                if (obstacle == null) return false;

                int propertyValue = GetObstaclePropertyValue(obstacle, mod.Context);
                return propertyValue <= mod.Threshold;  // Inverted: lower is better

            case ModifierType.HasItemCategory:
                // Equipment category presence (MECHANICAL PROPERTY, NOT ID)
                List<ItemCategory> categories = GetPlayerEquipmentCategories(player, itemRepository);
                if (Enum.TryParse<ItemCategory>(mod.Context, out ItemCategory targetCategory))
                {
                    return categories.Contains(targetCategory);
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Get all equipment categories from player inventory
    /// Same pattern as RouteOption.cs:305-323 (CORRECT PATTERN)
    /// NO ID MATCHING: Checks mechanical properties, not specific item IDs
    /// </summary>
    private List<ItemCategory> GetPlayerEquipmentCategories(Player player, ItemRepository itemRepository)
    {
        List<ItemCategory> categories = new List<ItemCategory>();

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (string.IsNullOrEmpty(itemId)) continue;

            Item item = itemRepository.GetItemById(itemId);
            if (item != null)
            {
                categories.AddRange(item.Categories);  // MECHANICAL PROPERTIES
            }
        }

        return categories.Distinct().ToList();
    }

    /// <summary>
    /// Find parent obstacle for a goal
    /// Goal placement doesn't determine ownership - must search GameWorld.Obstacles
    /// </summary>
    private Obstacle FindParentObstacle(Goal goal)
    {
        // Search all obstacles in GameWorld for one that contains this goal
        foreach (Obstacle obstacle in _gameWorld.Obstacles)
        {
            if (obstacle.GoalIds != null && obstacle.GoalIds.Contains(goal.Id))
            {
                return obstacle;
            }
        }
        return null;
    }

    /// <summary>
    /// Get obstacle property value by name
    /// </summary>
    private int GetObstaclePropertyValue(Obstacle obstacle, string propertyName)
    {
        // All obstacle types now use single Intensity property
        return obstacle.Intensity;
    }

    /// <summary>
    /// Format modifier for UI display
    /// </summary>
    private string FormatModifier(DifficultyModifier mod, bool applied)
    {
        string effectStr = mod.Effect >= 0 ? $"+{mod.Effect}" : $"{mod.Effect}";
        string status = applied ? "✓" : "✗";

        switch (mod.Type)
        {
            case ModifierType.Understanding:
                return $"{status} Understanding ≥ {mod.Threshold}: {effectStr}";

            case ModifierType.Mastery:
                return $"{status} Mastery({mod.Context}) ≥ {mod.Threshold}: {effectStr}";

            case ModifierType.Familiarity:
                return $"{status} Familiarity(location) ≥ {mod.Threshold}: {effectStr}";

            case ModifierType.ConnectionTokens:
                return $"{status} Connection(NPC) ≥ {mod.Threshold}: {effectStr}";

            case ModifierType.ObstacleProperty:
                return $"{status} {mod.Context} ≤ {mod.Threshold}: {effectStr}";

            case ModifierType.HasItemCategory:
                return $"{status} Has {mod.Context}: {effectStr}";

            default:
                return $"{status} {mod.Type}: {effectStr}";
        }
    }
}

/// <summary>
/// Result of difficulty calculation (for UI display)
/// Shows transparent breakdown of base difficulty and all modifiers
/// </summary>
public class DifficultyResult
{
    /// <summary>
    /// Base difficulty before any modifiers
    /// </summary>
    public int BaseDifficulty { get; set; }

    /// <summary>
    /// Final difficulty after applying modifiers
    /// Minimum 0 (can't go negative)
    /// </summary>
    public int FinalDifficulty { get; set; }

    /// <summary>
    /// Modifiers that applied (player met threshold)
    /// Formatted strings for UI display
    /// Example: "✓ Understanding ≥ 2: -3"
    /// </summary>
    public List<string> AppliedModifiers { get; set; } = new List<string>();

    /// <summary>
    /// Modifiers that didn't apply (player didn't meet threshold)
    /// Formatted strings for UI display
    /// Example: "✗ Familiarity(mill) ≥ 2: -2"
    /// Shows player what they need to reduce difficulty further
    /// </summary>
    public List<string> UnappliedModifiers { get; set; } = new List<string>();

    /// <summary>
    /// Total difficulty reduction from modifiers
    /// </summary>
    public int TotalReduction => BaseDifficulty - FinalDifficulty;
}
