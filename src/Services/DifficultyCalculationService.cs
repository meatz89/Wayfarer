/// <summary>
/// Service for calculating final difficulty based on base difficulty and player state
/// Replaces SituationRequirementsChecker (which was boolean gate system)
/// No boolean gates: All situations always visible, difficulty varies transparently
/// </summary>
public class DifficultyCalculationService
{
    private readonly GameWorld _gameWorld;

    public DifficultyCalculationService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Calculate final difficulty for a situation
    /// Returns base difficulty plus/minus modifiers
    /// Situation ALWAYS visible regardless of difficulty
    /// </summary>
    public DifficultyResult CalculateDifficulty(Situation situation, int baseDifficulty, ItemRepository itemRepository)
    {
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));

        Player player = _gameWorld.GetPlayer();
        int finalDifficulty = baseDifficulty;
        List<string> appliedModifiers = new List<string>();
        List<string> unappliedModifiers = new List<string>();

        foreach (DifficultyModifier mod in situation.DifficultyModifiers)
        {
            if (CheckModifier(mod, player, situation, itemRepository))
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
            BaseDifficulty = baseDifficulty,
            FinalDifficulty = Math.Max(0, finalDifficulty),
            AppliedModifiers = appliedModifiers,
            UnappliedModifiers = unappliedModifiers
        };
    }

    /// <summary>
    /// Check if modifier threshold is met
    /// NO ID MATCHING: Only mechanical properties and numerical resources
    /// </summary>
    private bool CheckModifier(DifficultyModifier mod, Player player, Situation situation, ItemRepository itemRepository)
    {
        switch (mod.Type)
        {
            case ModifierType.Understanding:
                // Global Mental expertise (0-10)
                return player.Understanding >= mod.Threshold;

            case ModifierType.Mastery:
                // Physical expertise per challenge type (uses DeckId, not Type)
                int mastery = player.MasteryCubes.GetMastery(mod.Context);
                return mastery >= mod.Threshold;

            case ModifierType.Familiarity:
                // Location understanding (0-3 per Location)
                // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
                // HIGHLANDER: Pass Location object to familiarity method (not Location.Id)
                if (situation.Location == null) return false;
                int familiarity = player.GetLocationFamiliarity(situation.Location);
                return familiarity >= mod.Threshold;

            case ModifierType.ConnectionTokens:
                // NPC relationship strength (0-15 per NPC)
                // ARCHITECTURAL CHANGE: Direct property access (situation owns placement)
                if (situation.Npc == null) return false;
                int tokens = player.GetNPCTokenCount(situation.Npc.ID, ConnectionType.Trust);
                return tokens >= mod.Threshold;

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
