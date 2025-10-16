using Wayfarer.GameState.Enums;

/// <summary>
/// Equipment usage type - determines item lifecycle
/// </summary>
public enum EquipmentUsageType
{
    Permanent,      // Always functional, never destroyed (Rope, Waders, Lantern, Quality Clothing)
    Consumable      // Single use, destroyed after use (Rations, Healing items)
}

public class Equipment : Item
{
    public List<ObstacleContext> ApplicableContexts { get; set; } = new List<ObstacleContext>();
    public int IntensityReduction { get; set; } = 0;

    // Equipment usage type - Permanent (always works) or Consumable (single use)
    public EquipmentUsageType UsageType { get; set; } = EquipmentUsageType.Permanent;

    public bool MatchesContext(ObstacleContext context)
    {
        return ApplicableContexts.Contains(context);
    }

    public static Equipment FromItem(Item item, List<ObstacleContext> applicableContexts = null, int intensityReduction = 0)
    {
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
            ApplicableContexts = applicableContexts ?? new List<ObstacleContext>(),
            IntensityReduction = intensityReduction
        };

        return equipment;
    }
}
