using Wayfarer.GameState.Enums;

/// <summary>
/// Equipment usage type - determines item lifecycle
/// </summary>
public enum EquipmentUsageType
{
    Permanent,      // Always functional, never destroyed (Rope, Waders, Lantern, Quality Clothing)
    Consumable,     // Single use, destroyed after use (Rations, Healing items)
    Exhaustible     // Multi-use with exhaustion, repairable (Climbing Gear - 3 uses, 25 coins to repair)
}

/// <summary>
/// Equipment functional state - determines if equipment can be used
/// </summary>
public enum EquipmentState
{
    Functional,     // Equipment works normally
    Exhausted       // Equipment needs repair before use
}

public class Equipment : Item
{
    public List<ObstacleContext> ApplicableContexts { get; set; } = new List<ObstacleContext>();
    public int IntensityReduction { get; set; } = 0;

    // Equipment usage type - Permanent (always works) or Consumable (single use)
    public EquipmentUsageType UsageType { get; set; } = EquipmentUsageType.Permanent;

    // EXHAUSTION SYSTEM (for Exhaustible equipment only)
    /// <summary>
    /// Number of uses before equipment becomes Exhausted (Exhaustible only)
    /// </summary>
    public int ExhaustAfterUses { get; set; } = 0;

    /// <summary>
    /// Current number of uses (0 = fresh, ExhaustAfterUses = exhausted)
    /// </summary>
    public int CurrentUses { get; set; } = 0;

    /// <summary>
    /// Coins required to repair this equipment when Exhausted
    /// </summary>
    public int RepairCost { get; set; } = 0;

    /// <summary>
    /// Current functional state (Functional or Exhausted)
    /// </summary>
    public EquipmentState CurrentState { get; set; } = EquipmentState.Functional;

    public bool MatchesContext(ObstacleContext context)
    {
        return ApplicableContexts.Contains(context);
    }

    public static Equipment FromItem(Item item, List<ObstacleContext> applicableContexts = null, int intensityReduction = 0)
    {
        if (item == null)
            throw new System.ArgumentNullException(nameof(item));

        Equipment equipment = new Equipment
        {
            Id = item.Id,
            Name = item.Name,
            InitiativeCost = item.InitiativeCost,
            BuyPrice = item.BuyPrice,
            SellPrice = item.SellPrice,
            Categories = item.Categories,
            Weight = item.Weight,
            Size = item.Size,
            VenueId = item.VenueId,
            LocationId = item.LocationId,
            Description = item.Description,
            TokenGenerationModifiers = item.TokenGenerationModifiers,
            EnablesTokenGeneration = item.EnablesTokenGeneration,
            IsAvailable = item.IsAvailable,
            ApplicableContexts = applicableContexts,
            IntensityReduction = intensityReduction
        };

        return equipment;
    }
}
