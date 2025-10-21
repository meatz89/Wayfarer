using System;
using Wayfarer.GameState.Enums;

/// <summary>
/// Catalog translating categorical durability properties to mechanical values
/// Pattern: Categorical (fiction) → Mechanical (game design)
/// </summary>
public static class EquipmentDurabilityCatalog
{
    /// <summary>
    /// Get mechanical durability values from categorical property
    /// </summary>
    public static (int exhaustAfterUses, int repairCost) GetDurabilityValues(DurabilityType durability)
    {
        return durability switch
        {
            DurabilityType.Fragile => (2, 10),   // Delicate tools, simple rope
            DurabilityType.Sturdy => (5, 25),    // Quality tools, climbing gear
            DurabilityType.Durable => (8, 40),   // Masterwork equipment, reinforced gear
            _ => throw new InvalidOperationException($"Unknown durability type: {durability}")
        };
    }
}
